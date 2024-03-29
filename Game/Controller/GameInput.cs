﻿using System;
using System.Linq;
using DigBuild.Platform.Input;
using DigBuild.Ui;

namespace DigBuild.Controller
{
    /// <summary>
    /// The main game input handler.
    /// </summary>
    public class GameInput
    {
        private Platform.Input.Controller? _controller;
        private bool _keyW, _keyA, _keyS, _keyD, _keySpace;
        private uint _cursorX, _cursorY, _prevCursorX, _prevCursorY;
        private bool _btnL, _btnR;
        private double _accumulatedScroll;
        
        public float PitchDelta, YawDelta, ForwardDelta, SidewaysDelta;
        public bool Jump;
        
        public bool PrevActivate, Activate;
        public bool PrevPunch, Punch;

        public bool PrevCycleLeft, CycleLeft;
        public bool PrevCycleRight, CycleRight;

        public bool PrevSwapUp, SwapUp;
        public bool PrevSwapDown, SwapDown;

        public bool CloseUi, PrevCloseUi;

        /// <summary>
        /// Handles key presses.
        /// </summary>
        /// <param name="code">Key code</param>
        /// <param name="action">Key action</param>
        public void OnKeyboardEvent(uint code, KeyboardAction action)
        {
            if (code == 17)
                _keyW = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keyW);
            if (code == 30)
                _keyA = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keyA);
            if (code == 31)
                _keyS = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keyS);
            if (code == 32)
                _keyD = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keyD);
            if (code == 57)
                _keySpace = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keySpace);
        }

        /// <summary>
        /// Handles cursor movement.
        /// </summary>
        /// <param name="x">The X position</param>
        /// <param name="y">The Y position</param>
        public void OnCursorMoved(uint x, uint y)
        {
            _cursorX = x;
            _cursorY = y;
        }

        /// <summary>
        /// Handles mouse button events.
        /// </summary>
        /// <param name="button">The button</param>
        /// <param name="action">The action</param>
        public void OnMouseEvent(uint button, MouseAction action)
        {
            if (button == 0)
                _btnL = action == MouseAction.Press;
            if (button == 1)
                _btnR = action == MouseAction.Press;
        }

        /// <summary>
        /// Handles scroll events.
        /// </summary>
        /// <param name="xOffset">The X scroll offset</param>
        /// <param name="yOffset">The Y scroll offset</param>
        public void OnScrollEvent(double xOffset, double yOffset)
        {
            _accumulatedScroll += yOffset;
        }

        /// <summary>
        /// Updates the game input and checks for controller data.
        /// </summary>
        /// <param name="uiManager">The UI manager</param>
        public void Update(UiManager uiManager)
        {
            Platform.Platform.InputContext.Update();
            _controller ??= Platform.Platform.InputContext.Controllers.FirstOrDefault();
            
            var cursorDeltaX = (int) (_cursorX - _prevCursorX) * 0.0125f;
            var cursorDeltaY = (int) (_cursorY - _prevCursorY) * 0.0125f;

            var hasController = _controller is { Connected: true };
            YawDelta = Bias(hasController ? _controller!.Joysticks[2] : 0, true) + cursorDeltaX;
            PitchDelta = -Bias(hasController ? _controller!.Joysticks[3] : 0, true) - cursorDeltaY;
            ForwardDelta = _keyS ? -1 : _keyW ? 1 : -Bias(hasController ? _controller!.Joysticks[1] : 0);
            SidewaysDelta = _keyA ? -1 : _keyD ? 1 : Bias(hasController ? _controller!.Joysticks[0] : 0);
            Jump = _keySpace || (hasController && _controller!.Buttons[5]);
            
            PrevActivate = Activate;
            Activate = (hasController && _controller!.Buttons[0]) || _btnL;

            PrevPunch = Punch;
            Punch = (hasController && _controller!.Buttons[1]) || _btnR;
            
            PrevCycleLeft = CycleLeft;
            PrevCycleRight = CycleRight;
            PrevSwapUp = SwapUp;
            PrevSwapDown = SwapDown;
            if (hasController)
            {
                (SwapUp, CycleRight, SwapDown, CycleLeft) = _controller!.Hats[0];
            }
            else
            {
                SwapUp = SwapDown = false;
                CycleRight = _accumulatedScroll < 0;
                CycleLeft = _accumulatedScroll > 0;
            }

            PrevCloseUi = CloseUi;
            CloseUi = hasController && _controller!.Buttons[7];
            if (CloseUi && !PrevCloseUi)
            {
                if (uiManager.Uis.Count() > 1)
                    uiManager.CloseTop();
                else
                    uiManager.Open(MenuUi.Create());
            }

            _prevCursorX = _cursorX;
            _prevCursorY = _cursorY;
            _accumulatedScroll = 0;
        }

        private static float Bias(float value, bool scale = false)
        {
            if (Math.Abs(value) < 0.1F) return 0;
            return scale ? MathF.CopySign(value * value, value) : value;
        }
    }
}
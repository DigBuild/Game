using System;
using System.Linq;
using DigBuild.Platform.Input;

namespace DigBuild.Controller
{
    public class GameInput
    {
        private Platform.Input.Controller? _controller;
        private bool _keyW, _keyA, _keyS, _keyD, _keySpace;
        private uint _cursorX, _cursorY, _prevCursorX, _prevCursorY;
        private bool _btnL, _btnR;

        private bool _keyQ;
        public static bool ReRender;

        public float PitchDelta, YawDelta, ForwardDelta, SidewaysDelta;
        public bool Jump;
        
        public bool PrevActivate, Activate;
        public bool PrevPunch, Punch;

        public bool PrevCycleLeft, CycleLeft;
        public bool PrevCycleRight, CycleRight;

        public bool PrevSwapUp, SwapUp;
        public bool PrevSwapDown, SwapDown;

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

            if (code == 16)
            {
                var wasPressed = _keyQ;
                _keyQ = action == KeyboardAction.Press || (action != KeyboardAction.Release && _keyQ);
                ReRender = !wasPressed && _keyQ;
            }
        }

        public void OnCursorMoved(uint x, uint y, CursorAction action)
        {
            _cursorX = x;
            _cursorY = y;
        }

        public void OnMouseEvent(uint button, MouseAction action)
        {
            if (button == 0)
                _btnL = action == MouseAction.Press;
            if (button == 1)
                _btnR = action == MouseAction.Press;
        }

        public void Update()
        {
            Platform.Platform.InputContext.Update();
            _controller ??= Platform.Platform.InputContext.Controllers.FirstOrDefault();
            
            var cursorDeltaX = (int) (_cursorX - _prevCursorX) * 0.0125f;
            var cursorDeltaY = (int) (_cursorY - _prevCursorY) * 0.0125f;

            var hasController = _controller is { Connected: true };
            YawDelta = Bias(hasController ? _controller!.Joysticks[2] : 0) + cursorDeltaX;
            PitchDelta = -Bias(hasController ? _controller!.Joysticks[3] : 0) - cursorDeltaY;
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
                (SwapUp, CycleRight, SwapDown, CycleLeft) = _controller!.Hats[0];
            else
                SwapUp = CycleRight = SwapDown = CycleLeft = false;

            _prevCursorX = _cursorX;
            _prevCursorY = _cursorY;
        }

        private static float Bias(float value)
        {
            if (Math.Abs(value) < 0.1F) return 0;
            return value;
        }
    }
}
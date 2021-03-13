﻿using System;
using System.Linq;
using DigBuild.Platform.Input;

namespace DigBuild
{
    public class GameInput
    {
        private Controller? _controller;
        public float PitchDelta, YawDelta, ForwardDelta, SidewaysDelta;
        public bool Jump;
        
        public bool PrevActivate, Activate;
        public bool PrevPunch, Punch;

        public bool PrevCycleLeft, CycleLeft;
        public bool PrevCycleRight, CycleRight;

        public void Update()
        {
            Platform.Platform.InputContext.Update();
            _controller ??= Platform.Platform.InputContext.Controllers.FirstOrDefault();
            if (_controller == null)
                return;
        
            PitchDelta = -Bias(_controller.Joysticks[3]);
            YawDelta = Bias(_controller.Joysticks[2]);
            ForwardDelta = -Bias(_controller.Joysticks[1]);
            SidewaysDelta = Bias(_controller.Joysticks[0]);
            Jump = _controller.Buttons[5];
            
            PrevActivate = Activate;
            Activate = _controller.Buttons[0];

            PrevPunch = Punch;
            Punch = _controller.Buttons[1];
            
            PrevCycleLeft = CycleLeft;
            CycleLeft = _controller.Hats[0].Has(Controller.HatState.Left);
            PrevCycleRight = CycleRight;
            CycleRight = _controller.Hats[0].Has(Controller.HatState.Right);
        }

        private static float Bias(float value)
        {
            if (Math.Abs(value) < 0.1F) return 0;
            return value;
        }
    }
}
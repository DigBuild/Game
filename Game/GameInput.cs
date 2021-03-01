﻿using System;
using System.Linq;
using DigBuildPlatformCS;
using DigBuildPlatformCS.Input;

namespace DigBuild
{
    public class GameInput
    {
        private Controller? _controller;
        public float PitchDelta, YawDelta, ForwardDelta, SidewaysDelta;
        public bool Jump;

        public void Update()
        {
            Platform.InputContext.Update();
            _controller ??= Platform.InputContext.Controllers.FirstOrDefault();
            if (_controller == null)
                return;
        
            PitchDelta = Bias(_controller.Joysticks[3]);
            YawDelta = Bias(_controller.Joysticks[2]);
            ForwardDelta = -Bias(_controller.Joysticks[1]);
            SidewaysDelta = Bias(_controller.Joysticks[0]);
            Jump = _controller.Buttons[5];
        }

        private static float Bias(float value)
        {
            if (Math.Abs(value) < 0.1F) return 0;
            return value;
        }
    }
}
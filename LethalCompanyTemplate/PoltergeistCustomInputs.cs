using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace Poltergeist
{
    public class PoltergeistCustomInputs : LcInputActions
    {
        public static PoltergeistCustomInputs instance {get; private set; }

        [InputAction("<Mouse>/leftButton", Name = "Toggle Ghost Light")]
        public InputAction SwitchLightButton { get; private set; }

        [InputAction("<Mouse>/scroll/up", Name = "Accelerate")]
        public InputAction AccelerateButton { get; private set; }

        [InputAction("<Mouse>/scroll/down", Name = "Decelerate")]
        public InputAction DecelerateButton { get; private set; }

        [InputAction("<Keyboard>/e", Name = "Ghost Interact")]
        public InputAction InteractButton { get; private set; }

        [InputAction("<Keyboard>/q", Name = "Toggle Spectate Mode")]
        public InputAction ToggleButton { get; private set; }

        [InputAction("<Keyboard>/r", Name = "Up")]
        public InputAction UpKey { get; private set; }

        [InputAction("<Keyboard>/f", Name = "Down")]
        public InputAction DownKey { get; private set; }

        [InputAction("<Keyboard>/l", Name = "Lock Altitude")]
        public InputAction LockKey { get; private set; }

        //When any instance is constructed, set it to be the instance
        public PoltergeistCustomInputs() : base()
        {
            instance = this;
        }
    }
}

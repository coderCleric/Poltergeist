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

        [InputAction("<Keyboard>/c", Name = "Manifest")]
        public InputAction ManifestKey { get; private set; }

        //When any instance is constructed, set it to be the instance
        public PoltergeistCustomInputs() : base()
        {
            instance = this;
        }

        //Get the string representation of the interact key
        public static string GetInteractString()
        {
            string str = PoltergeistCustomInputs.instance.InteractButton.GetBindingDisplayString();
            string[] parts = str.Split(" | ");
            bool useKBM = false;
            bool useGamepad = false;

            //Is there a keyboard key?
            if (parts[0].Length > 0)
                useKBM = true;

            //Is there a gamepad key?
            if (parts[1].Length > 0)
                useGamepad = true;

            //Construct the string
            string retStr = "";
            if (useKBM)
                retStr += parts[0];
            if (useKBM && useGamepad)
                retStr += " | ";
            if (useGamepad)
                retStr += parts[1];

            return retStr;
        }
    }
}

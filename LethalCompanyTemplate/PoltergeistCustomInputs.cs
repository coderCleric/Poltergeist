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

        [InputAction("<Keyboard>/v", Name = "Play Audio")]
        public InputAction BarkKey { get; private set; }

        [InputAction("<Keyboard>/h", Name = "Toggle Controls")]
        public InputAction ToggleControlsKey { get; private set; }

        //When any instance is constructed, set it to be the instance
        public PoltergeistCustomInputs() : base()
        {
            instance = this;
        }

        //Get the string representation of the interact key
        public static string GetInteractString()
        {
            return GetKeyString(instance.InteractButton);
        }

        //Get the string representation of the any key
        //If there's no binding, it's ""
        //If there's both, it's "KBM | CTR"
        //If there's only keyboard, it's "KBM | "
        //IF there's only controller, it's "CTR"
        public static string GetKeyString(InputAction action)
        {
            string str = action.GetBindingDisplayString();

            //If there's no string, that means the control is unbound
            if (str.Length <= 0)
                return "None";

            //If there's no vertical bar, just display it
            if (!str.Contains("|"))
                return str;

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

using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode.Components;
using UnityEngine;

namespace Poltergeist
{
    [DisallowMultipleComponent]
    public class HeadTransform : NetworkTransform
    {
        /**
         * This should make it so that the client can actually move it
         */
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}

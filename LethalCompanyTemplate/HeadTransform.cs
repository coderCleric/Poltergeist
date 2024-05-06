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
         * Set up the attributes that we want
         */
        protected override void Awake()
        {
            base.Awake();

            SyncScaleX = false;
            SyncScaleY = false;
            SyncScaleZ = false;
        }

        /**
         * This should make it so that the client can actually move it
         */
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}

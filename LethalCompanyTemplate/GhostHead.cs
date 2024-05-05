using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace Poltergeist
{
    public class GhostHead : NetworkBehaviour
    {
        //Mapping between player controllers and head objects, only need to maintain on the host
        public static Dictionary<PlayerControllerB, GhostHead> headMapping = new Dictionary<PlayerControllerB, GhostHead> ();
    }
}

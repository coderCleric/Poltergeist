using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist
{
    public class GhostFollowTrigger : MonoBehaviour
    {
        /**
         * When the host ghost touches the trigger, set the cam to follow
         */
        private void OnTriggerEnter(Collider other)
        {
            GhostHead head = other.GetComponent<GhostHead>();
            if (head != null && head.isHostHead)
            {
                SpectatorCamController.instance.ParentTo(transform);
            }
        }
        /**
         * When the host ghost leaves the trigger, make the cam not follow
         */
        private void OnTriggerExit(Collider other)
        {
            GhostHead head = other.GetComponent<GhostHead>();
            if (head != null && head.isHostHead)
            {
                SpectatorCamController.instance.ParentTo(null);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist
{
    public class GhostItemTrigger : MonoBehaviour
    {
        GrabbableObject itemScript;

        /**
         * On start, grab needed components
         */
        private void Start()
        {
            itemScript = transform.parent.GetComponent<GrabbableObject>();
        }

        /**
         * When a ghost touches the item, activate it
         */
        private void OnTriggerEnter(Collider other)
        {
            //Purposefully allowing it to be held for the extra spooks!
            if (other.gameObject.GetComponent<GhostHead>() != null)
            {
                itemScript.ActivatePhysicsTrigger(other);
            }
        }
    }
}

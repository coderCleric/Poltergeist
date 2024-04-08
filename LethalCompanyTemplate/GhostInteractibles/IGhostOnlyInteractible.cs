using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles
{
    public interface IGhostOnlyInteractible : IGhostInteractible
    {
        protected static List<IGhostOnlyInteractible> managedInteractibles = new List<IGhostOnlyInteractible>();

        //Activates the interactible
        public void SetActivation(bool activation);

        /**
         * Registers this interactible in the list
         */
        public static void Register(IGhostOnlyInteractible interactible)
        {
            managedInteractibles.Add(interactible);
        }

        /**
         * Unregisters this interactible from the list
         */
        public static void Unregister(IGhostOnlyInteractible interactible)
        {
            managedInteractibles.Remove(interactible);
        }

        /**
         * Toggles the raycast colliders on all of the ghost interactibles
         */
        public static void SetGhostActivation(bool active)
        {
            //Loop through each registered interacible
            foreach (IGhostOnlyInteractible interactible in managedInteractibles)
            {
                interactible.SetActivation(active);
            }
        }
    }
}

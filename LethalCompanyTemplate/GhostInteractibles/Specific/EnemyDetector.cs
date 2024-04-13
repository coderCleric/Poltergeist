using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class EnemyDetector : NaiveInteractible
    {
        public EnemyInteractible enemyInteractible;

        /**
         * Tells this detector what interactible it belongs to
         */
        public void RegisterInteractible(EnemyInteractible enemyInteractible)
        {
            this.enemyInteractible = enemyInteractible;
        }

        /**
         * Cost is just cost of owner
         */
        public override float GetCost()
        {
            return enemyInteractible.GetCost();
        }

        /**
         * Interacting should just call the owners thing
         */
        public override float Interact(Transform playerTransform)
        {
            return enemyInteractible.Interact(playerTransform);
        }

        /**
         * String comes from owner
         */
        public override string GetTipText()
        {
            return enemyInteractible.GetTipText();
        }
    }
}

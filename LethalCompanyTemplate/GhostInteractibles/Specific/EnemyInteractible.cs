using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class EnemyInteractible : NaiveInteractible
    {
        private EnemyAI enemy;
        private float lastInteractTime = 0;

        /**
         * In awake, grab the enemy AI
         */
        private void Awake()
        {
            enemy = GetComponent<EnemyAICollisionDetect>().mainScript;
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            return 20;
        }

        /**
         * Do the actual interaction
         */
        public override float Interact(Transform playerTransform)
        {
            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Pester the enemy
            if (!enemy.isEnemyDead)
            {
                if (lastInteractTime + 3f < Time.time)
                    enemy.HitEnemyOnLocalClient(0, playHitSFX: true);
                else
                    enemy.HitEnemyOnLocalClient(0, playerWhoHit: enemy.GetClosestPlayer(false, false, false), playHitSFX: true);
                lastInteractTime = Time.time;
                return GetCost();
            }

            return 0;
        }

        /**
         * Gives the tip text
         */
        public override string GetTipText()
        {
            string retStr = "";

            //Display message for not having enough power
            if (SpectatorCamController.instance.Power < GetCost())
                return "Not Enough Power (" + GetCost().ToString("F0") + ")";

            //Set up the actual text
            if (!enemy.isEnemyDead)
                retStr = "Pester enemy : [E]";
            else
                return "Enemy is dead";

            return retStr + " (" + GetCost().ToString("F0") + ")";
        }
    }
}

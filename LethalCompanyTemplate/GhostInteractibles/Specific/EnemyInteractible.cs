using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Poltergeist.GhostInteractibles.Specific
{
    public class EnemyInteractible : NetworkedInteractible
    {
        private EnemyAI enemy;
        private float lastInteractTime = 0;
        private int consecutiveHits = 0;
        private EnemyDetector[] detectors;

        /**
         * Grab the enemy AI
         */
        protected override void DoSetup()
        {
            //If there is somehow no parent, try again later
            if (transform.parent == null)
            {
                waitTime = 2;
                wasBugged = true;
                Poltergeist.LogWarning($"An enemy interactible is orphaned! Trying setup again in {waitTime} seconds.\nThe host log should show what type of enemy is causing this.");
                SendWarningServerRpc();
                return;
            }

            //Find the enemy AI
            Transform enemyTF = transform.parent;
            enemy = enemyTF.GetComponent<EnemyAI>();

            //Make all of the detectors
            EnemyAICollisionDetect[] AIDetectors = enemyTF.GetComponentsInChildren<EnemyAICollisionDetect>();
            int detectorCount = AIDetectors.Length;
            detectors = new EnemyDetector[detectorCount];
            for (int i = 0; i < detectorCount; i++)
            {
                detectors[i] = AIDetectors[i].gameObject.AddComponent<EnemyDetector>();
                detectors[i].RegisterInteractible(this);
            }

            if (wasBugged)
                Poltergeist.Log($"Bugged enemy {gameObject.name} was recovered");
        }

        /**
         * Get the cost of the interaction
         */
        public override float GetCost()
        {
            return Poltergeist.Config.PesterCost.Value;
        }

        /**
         * Do the actual interaction
         */
        public override float Interact(Transform playerTransform)
        {
            //Abort if there's no enemy somehow
            if (enemy == null)
                return 0;

            //Don't let them interact without meeting the cost
            if (SpectatorCamController.instance.Power < GetCost())
                return 0;

            //Nothing happens if the enemy is dead
            if (enemy == null || enemy.isEnemyDead)
                return 0;

            //Handle the consecutive hit mechanic
            if (lastInteractTime + Poltergeist.Config.TimeForAggro.Value < Time.time)
                consecutiveHits = 1;
            else
                consecutiveHits++;

            //Send the interaction out to the other players
            if (consecutiveHits < Poltergeist.Config.HitsForAggro.Value)
                InteractServerRpc(-1);
            else
            {
                Poltergeist.DebugLog("Pestered " + enemy.gameObject.name + " into targeting a player");
                PlayerControllerB accusedPlayer = enemy.GetClosestPlayer(true, true, true);
                if(accusedPlayer != null)
                    InteractServerRpc((int)accusedPlayer.playerClientId);
                else
                    InteractServerRpc(-1);
            }
            lastInteractTime = Time.time;

            return GetCost();
        }

        /**
         * Do only the local interaction stuff
         */
        public void InteractLocallyOnly(PlayerControllerB accusedPlayer)
        {
            //Abort if there's no enemy somehow
            if (enemy == null)
                return;

            Poltergeist.DebugLog("Interacting locally with " + enemy.gameObject.name);
            enemy.HitEnemy(0, accusedPlayer, true);
        }

        /**
         * Gives the tip text
         */
        public override string GetTipText()
        {
            //Abort if there's no enemy somehow
            if (enemy == null)
                return "Enemy is not synced!";

            string retStr = "";

            //Display message for the enemy being dead
            if (enemy == null || enemy.isEnemyDead)
                return "Enemy is dead";

            //Display message for not having enough power
            if (SpectatorCamController.instance.Power < GetCost())
                return "Not Enough Power (" + GetCost().ToString("F0") + ")";

            //Set up the actual text
            retStr = "Pester enemy : [" + PoltergeistCustomInputs.GetInteractString() + "]";

            return retStr + " (" + GetCost().ToString("F0") + ")";
        }

        /**
         * Lets the server message clients about an enemy being pestered by a ghost
         */
        [ClientRpc]
        public void InteractClientRpc(int accussedID)
        {
            //Have the specified player "attack" the enemy
            if (accussedID == -1)
                InteractLocallyOnly(null);
            else
                InteractLocallyOnly(StartOfRound.Instance.allPlayerScripts[accussedID]);
        }

        /**
         * Lets the client tell the server we're activating it
         */
        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc(int accussedID)
        {
            InteractClientRpc(accussedID);
        }
    }
}

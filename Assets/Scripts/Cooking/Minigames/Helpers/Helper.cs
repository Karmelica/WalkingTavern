using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using World;

namespace Cooking.Minigames.Helpers
{
    public abstract class Helper : NetworkBehaviour
    {
        public Transform spawnLocation;
        
        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            if (!IsServer)
            {
                enabled = false;
            }
        }

        public abstract void CompleteMinigame(List<MoveableObject> objectsToChange);
    }
}

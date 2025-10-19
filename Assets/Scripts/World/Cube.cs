using System;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Cube : NetworkBehaviour, IInteractable
    {
        private Transform _targetPos;
        
        public void PrimaryInteract(ref GameObject heldObject)
        {
            heldObject = gameObject;
            NetworkObject.Despawn(false);
            gameObject.SetActive(false);
        }
    }
}

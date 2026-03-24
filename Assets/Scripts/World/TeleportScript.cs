using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    public Transform targetTeleportPosition;
    [SerializeField] private TeleportScript target;
    private Teleportable _traveller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out _traveller))
        {
            _traveller.Teleport(target.targetTeleportPosition.position);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(_traveller)
        {
            _traveller = null;
        }
    }
}



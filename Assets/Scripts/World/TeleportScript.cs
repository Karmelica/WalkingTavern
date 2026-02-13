using System;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    private bool isOverlapping = false;
    private Transform teleportedObject;
    [SerializeField] private Transform target;
    private ITeleportable teleportable;
    
    
    // Update is called once per frame
    void Update()
    {
        if (isOverlapping && teleportedObject)
        {
            Vector3 portalToObject = teleportedObject.position - transform.position;
            Vector3 posOffset = portalToObject;
            var position = target.position + posOffset;
            teleportable.Teleport(position);
            isOverlapping = false;
            teleportable = null;

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<ITeleportable>(out teleportable)) return;
        isOverlapping = true;
        teleportedObject = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        isOverlapping = true;
        teleportable = null;
        teleportedObject = null;
    }
}

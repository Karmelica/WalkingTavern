using System;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    private bool isOverlapping = false;
    private Transform teleportedObject;
    [SerializeField] private Transform target;
    
    
    // Update is called once per frame
    void Update()
    {
        if (isOverlapping && teleportedObject)
        {
            Vector3 portalToObject = teleportedObject.position - transform.position;
            float dotProduct = Vector3.Dot(Vector3.forward, portalToObject);

            if (dotProduct < 0f)
            {
                Vector3 posOffset = portalToObject;
                teleportedObject.position = target.position + posOffset;
                isOverlapping = false;
            }
        }
    }
    

    private void OnTriggerEnter(Collider other)
    {
        isOverlapping = true;
        teleportedObject = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        isOverlapping = true;
        teleportedObject = null;
    }
}

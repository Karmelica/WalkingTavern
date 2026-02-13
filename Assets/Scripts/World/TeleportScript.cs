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
            Debug.DrawLine(teleportedObject.position, transform.position, Color.red, 5f);
            Debug.DrawLine(transform.position + transform.up, transform.position, Color.green, 5f);
            Debug.DrawLine(transform.position + transform.up, teleportedObject.position - transform.localPosition, Color.blue, 5f);
            Vector3 portalToObject = teleportedObject.position - transform.position;
            Vector3 posOffset = portalToObject;
            float dotProduct = Vector3.Dot(transform.position + transform.up, portalToObject);
            Debug.Log(dotProduct + ": position");
            dotProduct = Vector3.Dot(transform.localPosition + transform.up, portalToObject);
            Debug.Log(dotProduct + ": localPosition");
            /*if(dotProduct < 0)
            {
                var position = target.position + posOffset;
                teleportable.Teleport(position);
                isOverlapping = false;
                teleportable = null;
            }*/

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

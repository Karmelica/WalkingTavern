using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class TeleportScript : MonoBehaviour
{
    [SerializeField] private TeleportScript target;
    private Teleportable traveller;


    // Update is called once per frame
    private void LateUpdate()
    {
        if (traveller)
        {
            var m = target.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveller.transform.localToWorldMatrix;
            
            Vector3 portalToObject = traveller.transform.position - transform.position;

            int portalSide = System.Math.Sign(Vector3.Dot(transform.forward, portalToObject));
            int oldPortalSide = System.Math.Sign(Vector3.Dot(transform.forward, traveller.lastOffsetFromPortal));

            if (oldPortalSide != portalSide)
            {
                traveller.Teleport(m.GetColumn(3), m.rotation);
                target.EnterPortal(traveller);
                traveller = null;
            }
            else
            {
                traveller.lastOffsetFromPortal = portalToObject;
            }

        }
    }
    
    public void EnterPortal(Teleportable newTraveller)
    {
        var portalToObject = newTraveller.transform.position - transform.position;
        newTraveller.lastOffsetFromPortal = portalToObject;
        traveller = newTraveller;
    }

    private void OnTriggerEnter(Collider other)
    {
        var newTraveller = other.GetComponent<Teleportable>();
        if(newTraveller){
            EnterPortal(newTraveller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(traveller)
        {
            traveller = null;
        }
    }
}



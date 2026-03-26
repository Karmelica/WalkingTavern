using System;
using UnityEngine;

public class DeathZoneTeleporter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (Physics.Raycast(other.transform.position, Vector3.up * 300f, out RaycastHit hit))
        {
            other.transform.position = hit.point + Vector3.up;
            if(other.attachedRigidbody)
            {
                other.attachedRigidbody.linearDamping = Single.PositiveInfinity;
                other.attachedRigidbody.linearDamping = 0.1f;
            }
        }
    }
}

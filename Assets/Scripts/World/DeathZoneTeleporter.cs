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
                var oldDamping = other.attachedRigidbody.linearDamping;
                other.attachedRigidbody.linearDamping = float.PositiveInfinity;
                other.attachedRigidbody.linearDamping = oldDamping;
            }
        }
    }
}

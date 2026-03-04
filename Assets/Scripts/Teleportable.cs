using UnityEngine;

public class Teleportable : MonoBehaviour
{
    public Vector3 lastOffsetFromPortal;
    
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
    }
}
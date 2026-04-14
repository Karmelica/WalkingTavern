using UnityEngine;

public class Teleportable : MonoBehaviour
{
    public void Teleport(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        Physics.SyncTransforms();
    }
}
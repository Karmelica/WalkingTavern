using UnityEngine;

public class Teleportable : MonoBehaviour
{
    public Vector3 lastOffsetFromPortal;
    [SerializeField] private CharacterController myCenter;
    
    public void Teleport(Vector3 position)
    {
        transform.position = position;
    }
}
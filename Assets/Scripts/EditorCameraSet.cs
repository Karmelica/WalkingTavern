using System;
using Cooking.Minigames;
using UnityEngine;

[ExecuteAlways]
public class EditorCameraSet : MonoBehaviour
{
    private Minigame _minigame;
    private Transform cameraLocation;
    private Transform foodPlaceholder;
    
#if UNITY_EDITOR
    private void Start()
    {
        if (!TryGetComponent(out _minigame)) return;
        cameraLocation = _minigame.cameraLocation;
        foodPlaceholder = _minigame.foodPlaceholder;
    }

    private void Update()
    {
        if (!_minigame) return;
        EditorUpdate();
    }
    
    private void EditorUpdate()
    {
        cameraLocation.LookAt(foodPlaceholder);
    }
            
    private void OnDrawGizmos()
    {
        if (foodPlaceholder)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(foodPlaceholder.position, 0.1f);
        }
        if (cameraLocation)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = cameraLocation.localToWorldMatrix;
            Gizmos.DrawFrustum(cameraLocation.position, 60, 0.3f, 60, 16 / 9f);
        }
    }
#endif
}

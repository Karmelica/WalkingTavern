using System;
using UnityEngine;

public class CanvasToFacePlayer : MonoBehaviour
{
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mainCamera) return;
        transform.forward = mainCamera.transform.forward;
    }
}

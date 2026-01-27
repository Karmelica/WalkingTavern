using System;
using UnityEngine;

public class CanvasToFacePlayer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (!Camera.main) return;
        transform.forward = Camera.main.transform.forward;
    }
}

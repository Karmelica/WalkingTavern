using System;
using UnityEngine;

public class CanvasToFacePlayer : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}

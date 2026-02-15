using System;
using UnityEngine;

public class Room : MonoBehaviour
{
    public Vector3 windowOffset { get; private set; }
    [SerializeField] private Transform insideWindow;
    [SerializeField] private Transform outsideWindow;

    private void Start()
    {
        windowOffset = outsideWindow.position - insideWindow.position;
    }
}

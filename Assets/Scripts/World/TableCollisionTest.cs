using System;
using UnityEngine;

public class TableCollisionTest : MonoBehaviour
{ 
    public bool IsColliding { get; private set; } = true; 
    [SerializeField] private Material mat; 
    private Material matt;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        matt = new Material(mat);
        meshRenderer.materials = new[] { matt };
    }

    private void Update()
    {
        if (Physics.CheckBox(transform.position + Vector3.up, Vector3.one * 0.5f))
        {
            IsColliding = true;
            matt.color = new Color(255, 0, 0, 0.2f);
        }
        else
        {
            IsColliding = false;
            matt.color = new Color(0, 255, 0, 0.2f);
        }
    }
}

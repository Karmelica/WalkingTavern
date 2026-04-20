using System;
using UnityEngine;

public class TableCollisionTest : MonoBehaviour
{
    public bool IsColliding { get; private set; } = true; 
    [SerializeField] private Material mat; 
    private Material _matt;
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _matt = new Material(mat);
        _meshRenderer.material = _matt;
    }

    private void Update()
    {
        if (Physics.CheckBox(transform.position + Vector3.up, Vector3.one * 0.5f))
        {
            IsColliding = true;
            var color = new Color(1, 0, 0, 0.5f);
            _meshRenderer.material.color = color;
        }
        else
        {
            IsColliding = false;
            var color = new Color(0, 1f, 0, 0.5f);
            _meshRenderer.material.color = color;
        }
    }
}

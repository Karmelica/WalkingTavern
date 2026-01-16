using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    [SerializeField] private CharacterController controller;
    private NetworkVariable<Vector3> randomDestination = new NetworkVariable<Vector3>(new Vector3(0,0,0));
    [SerializeField] private Vector3 newDestination;
    private float changeDirectionInterval = 5f;
    private float _timer = 0f;
    [SerializeField] private bool isMoving = true;
    private float offset = 20f;
    private float moveSpeed = 3f;

    private void Start()
    {
        if(IsOwner) StartCoroutine(PickRandomDirection());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        controller.Move((randomDestination.Value - transform.position).normalized * (Time.deltaTime * moveSpeed));
        if (controller.isGrounded) return;
        controller.Move(Physics.gravity * Time.deltaTime);
    }

    private IEnumerator PickRandomDirection()
    {
        var wait = new WaitForSeconds(changeDirectionInterval);
        while (isMoving)
        {
            Vector3 destination = new Vector3(RandomRange(transform.position.x), 0, RandomRange(transform.position.z));
            newDestination = destination;
            randomDestination.Value = destination;
            yield return wait;
        }
    }

    private float RandomRange(float pos)
    {
        return UnityEngine.Random.Range(pos - offset, pos + offset);
    }
}

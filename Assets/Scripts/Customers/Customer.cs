using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController controller;
    [SerializeField] private bool isMoving = true;
    
    private NetworkVariable<Vector3> _randomDestination = new NetworkVariable<Vector3>(new Vector3(0,0,0));
    
    private const float ChangeDirectionInterval = 5f;
    private const float Offset = 20f;

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
        agent.SetDestination(_randomDestination.Value);
        if (controller.isGrounded) return;
        controller.Move(Physics.gravity * Time.deltaTime);
    }

    private IEnumerator PickRandomDirection()
    {
        var wait = new WaitForSeconds(ChangeDirectionInterval);
        while (isMoving)
        {
            Vector3 destination = new Vector3(RandomRange(transform.position.x), 0, RandomRange(transform.position.z));
            _randomDestination.Value = destination;
            yield return wait;
        }
    }

    private float RandomRange(float pos)
    {
        return UnityEngine.Random.Range(pos - Offset, pos + Offset);
    }
}

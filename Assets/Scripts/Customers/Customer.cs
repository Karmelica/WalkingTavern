using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController controller;
    [SerializeField] private bool isMoving = true;
    private List<Transform> _waypoints = new();
    
    private NetworkVariable<Vector3> _randomDestination = new NetworkVariable<Vector3>(new Vector3(0,0,0));
    
    private const float ChangeDirectionInterval = 5f;
    private const float Offset = 20f;
    private const float treshhold = 1.5f;

    public CustomerState state { get; private set; } = CustomerState.WaitingInLine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); 
    }

    private void Update()
    {
        Gravity();
        agent.stoppingDistance = state == CustomerState.WaitingInLine ? 3f : 0f;
    }

    private void Gravity()
    {
        if (controller.isGrounded) return;
        controller.Move(Physics.gravity * Time.deltaTime);
    }
    
    public void SetState(CustomerState newState)
    {
        state = newState;
    }
    
    public void SetDestination(Transform destination)
    {
        agent.SetDestination(destination.position);
    }
}

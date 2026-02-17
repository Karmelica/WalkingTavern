using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cooking.ScriptableObjects;
using Unity.Behavior;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour, IInteractable
{
    private CustomerState _customerState;
    [SerializeField] private List<GameObject> ears;
    
    [SerializeField] private BehaviorGraphAgent behaviorGraphAgent;
    private BlackboardReference _blackboardReference;
    
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;

    private Recipe _requestedRecipe;
    private List<Transform> _waypoints;
    private AIManager _aiManager;

    #region Unity Lifecycle

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        int rand = Random.Range(0, ears.Count);
        ears[rand].SetActive(true);

        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        // Remove from waiting line
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if(customerList.Contains(gameObject)) customerList.Remove(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);
    }

    public void DespawnCustomer()
    {
        if(IsOwner)
            DespawnServerRpc();
    }

    [Rpc(SendTo.Server)]
    private void DespawnServerRpc()
    {
        NetworkObject.Despawn();
        _aiManager.SpawnCustomer();
    }

    private void Update()
    {
        Gravity();
        
        animator.SetBool("IsGrounded", controller.isGrounded);
        
        if(IsOwner)
            SetAnimSpeedServerRpc(agent.velocity.magnitude);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SetAnimSpeedServerRpc(float velocityMagnitude)
    {
        SetAnimSpeedClientRpc(velocityMagnitude);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetAnimSpeedClientRpc(float velocityMagnitude)
    {
        animator.SetFloat("WalkSpeed", velocityMagnitude);
    }

    #endregion

    private void Gravity()
    {
        if (controller.isGrounded) return;
        controller.Move(Physics.gravity * Time.deltaTime);
    }

    #region Get/Set

    public void AssignVariables(AIManager manager, Recipe recipe, List<Transform> waypoints)
    {
        _aiManager = manager;
        _requestedRecipe = recipe;
        _waypoints = waypoints;
        
        
        _blackboardReference = behaviorGraphAgent.BlackboardReference;
        _blackboardReference.SetVariableValue("Customer", this);

        // Assign waypoints
        _blackboardReference.SetVariableValue("OrderingLocation", _waypoints[0]);
        _blackboardReference.SetVariableValue("LeaveLocation", _waypoints[1]);
        _blackboardReference.SetVariableValue("WaitingLocation", _waypoints[2]);

        // Add to waiting line
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if (!customerList.Contains(gameObject)) customerList.Add(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);

        // SetRecipe
        _blackboardReference.SetVariableValue("RequestedRecipe", _requestedRecipe);
    }

    public bool TryGetSeat(out Transform seat)
    {
        seat = _aiManager.TryGetAvailableSeat();
        if(seat != null)
        {
            return true;
        }

        return false;
    }
    
    public void ReturnSeat(Transform seat)
    {
        _aiManager.ReturnSeat(seat);
    }
    
    #endregion

    #region Interact

    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
    {
        //nothing
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        //nothing
    }

    public string GetInteractName()
    {
        GetStateServerRpc();
        return $"Customer\nCurrent Task: {_customerState}\nRequested Recipe: {_requestedRecipe.dishType}";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void GetStateServerRpc()
    {
        SetStateClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void SetStateClientRpc()
    {
        _blackboardReference.GetVariableValue("State", out _customerState);
    }

    public bool IsInteractedWith()
    {
        return false;
    }

    #endregion
}

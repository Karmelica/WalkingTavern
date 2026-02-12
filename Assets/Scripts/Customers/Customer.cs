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
    public CustomerState state { get; private set; } = CustomerState.Ordering;
    
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
        _blackboardReference = behaviorGraphAgent.BlackboardReference;
        _blackboardReference.SetVariableValue("Customer", this);
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if(!customerList.Contains(gameObject)) customerList.Add(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);
        _blackboardReference.SetVariableValue("OrderingLocation", _waypoints[0]);
        _blackboardReference.SetVariableValue("LeaveLocation", _waypoints[1]);
        _blackboardReference.SetVariableValue("WaitingLocation", _waypoints[2]);
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if(customerList.Contains(gameObject)) customerList.Remove(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);
    }

    private void Update()
    {
        Gravity();
        animator.SetFloat("WalkSpeed", agent.velocity.magnitude);
        animator.SetBool("IsGrounded", controller.isGrounded);
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
        _blackboardReference.GetVariableValue("State", out int customerState);
        _blackboardReference.SetVariableValue("State", customerState + 1);
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        _aiManager.DespawnCustomer(this);
    }

    public string GetInteractName()
    {
        return gameObject.name + " (" + state + ")";
    }

    public bool IsInteractedWith()
    {
        return false;
    }

    #endregion
}

using System;
using System.Collections.Generic;
using Managers;
using Unity.Behavior;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour, IInteractable
{
    [SerializeField] public List<GameObject> ears;
    [SerializeField] public List<Material> skins;
    [SerializeField] public List<Material> faces;
    [SerializeField] private SkinnedMeshRenderer[] customerMesh;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    
    private BehaviorGraphAgent _behaviorGraphAgent;
    private BlackboardReference _blackboardReference;
    
    private NavMeshAgent _agent;
    private CharacterController _controller;
    private Animator _animator;
    private AIManager _aiManager;

    public NetworkVariable<FixedString32Bytes> customerName;
    public NetworkVariable<DishType> requestedDish;
    public NetworkVariable<int> selectedEarsIndex;
    public NetworkVariable<int> selectedSkinIndex;
    public NetworkVariable<int> selectedFaceIndex;
    public NetworkVariable<bool> isBeingInteracted =  new (true);
    private List<Transform> _waypoints = new();

    #region Unity Lifecycle

    private void Awake()
    {
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        _agent =  GetComponent<NavMeshAgent>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
        ears[selectedEarsIndex.Value].SetActive(true);
        faceRenderer.materials = new[]{ new Material (faces[selectedFaceIndex.Value]) };
        foreach (var mesh in customerMesh)
        {
            mesh.materials = new[] { skins[selectedSkinIndex.Value] };
        }
        
        if (!IsOwner) return;

        _waypoints.Add(GameObject.FindGameObjectWithTag("Ordering").transform);
        _waypoints.Add(GameObject.FindGameObjectWithTag("Entrance").transform);
        _waypoints.Add(GameObject.FindGameObjectWithTag("Waiting").transform);
        
        _blackboardReference = _behaviorGraphAgent.BlackboardReference;
        _blackboardReference.SetVariableValue("Customer", this);

        // Assign waypoints
        _blackboardReference.SetVariableValue("OrderingLocation", _waypoints[0]);
        _blackboardReference.SetVariableValue("LeaveLocation", _waypoints[1]);
        _blackboardReference.SetVariableValue("WaitingLocation", _waypoints[2]);

        // Add to waiting line
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if (!customerList.Contains(gameObject)) customerList.Add(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);
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
        
        _animator.SetBool("IsGrounded", _controller.isGrounded);
        
        if(IsOwner)
            SetAnimSpeedServerRpc(_agent.velocity.magnitude);
    }
    
    #endregion

    #region Animation

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void SetAnimSpeedServerRpc(float velocityMagnitude)
    {
        SetAnimSpeedClientRpc(velocityMagnitude);
    }

    [Rpc(SendTo.Everyone)]
    private void SetAnimSpeedClientRpc(float velocityMagnitude)
    {
        _animator.SetFloat("WalkSpeed", velocityMagnitude);
    }

    #endregion

    private void Gravity()
    {
        if (_controller.isGrounded) return;
        _controller.Move(Physics.gravity * Time.deltaTime);
    }

    #region Get/Set

    public void AssignVariables(AIManager manager)
    {
        _aiManager = manager;
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

    public IInteractable PrimaryInteract(NetworkBehaviourReference interactor, bool startedInteraction = true)
    {
        return null;
    }

    public IInteractable SecondaryInteract(NetworkBehaviourReference interactor)
    {
        TakeOrderRpc();
        return null;
    }

    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
    private void TakeOrderRpc()
    {
        _blackboardReference.GetVariableValue("Ordering", out CustomerState orderingState);
        if(orderingState == CustomerState.Ordering){
            _blackboardReference.SetVariableValue("Ordered", true);
        }
    }

    public string GetInteractName()
    {
        return $"Customer\nRequested Dish: {requestedDish.Value}";
    }

    public bool IsInteractedWith()
    {
        return isBeingInteracted.Value;
    }

    #endregion
}

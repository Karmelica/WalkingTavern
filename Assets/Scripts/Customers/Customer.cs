using System;
using System.Collections.Generic;
using System.Threading;
using Cooking;
using Managers;
using MyInterfaces;
using PlayerScripts;
using TMPro;
using Unity.Behavior;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Customer : NetworkBehaviour, IInteractable
{
    [SerializeField] public List<Material> skins;
    [SerializeField] public List<Material> faces;
    [SerializeField] public List<Material> clothesMats;
    [SerializeField] public List<Material> hairMats;
    [SerializeField] public SkinnedMeshRenderer[] ears;
    [SerializeField] public SkinnedMeshRenderer[] shirtMesh;
    [SerializeField] public SkinnedMeshRenderer[] pantsMesh;
    [SerializeField] public SkinnedMeshRenderer[] hairMesh;
    [SerializeField] private SkinnedMeshRenderer[] customerMesh;
    [SerializeField] private SkinnedMeshRenderer faceRenderer;
    [SerializeField] private Image foodIcon;
    [SerializeField] private Image timeImage;
    [SerializeField] private TextMeshProUGUI messageText;
    private Awaitable _messageAwaitable;
    private Awaitable _timerAwaitable;
    
    private BehaviorGraphAgent _behaviorGraphAgent;
    private BlackboardReference _blackboardReference;
    
    private NavMeshAgent _agent;
    private CharacterController _controller;
    private Animator _animator;
    private AIManager _aiManager;

    public NetworkVariable<FixedString64Bytes> customerName;
    public NetworkVariable<DishType> requestedDish;
    public NetworkVariable<int> selectedEarsIndex;
    public NetworkVariable<int> selectedSkinIndex;
    public NetworkVariable<int> selectedFaceIndex;
    public NetworkVariable<int> selectedPantsIndex;
    public NetworkVariable<int> selectedShirtIndex;
    public NetworkVariable<int> selectedHairIndex;
    public NetworkVariable<int> selectedPantsMat;
    public NetworkVariable<int> selectedShirtMat;
    public NetworkVariable<int> selectedHairMat;
    public NetworkVariable<bool> orderTaken =  new (true);
    private readonly List<Transform> _waypoints = new();
    private float _totalWaitTime;
    private float _elapsedTime;


    #region Unity Lifecycle
    
    private void Awake()
    {
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        _agent =  GetComponent<NavMeshAgent>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        Gravity();
        
        _animator.SetBool("IsGrounded", _controller.isGrounded);
        
        if(IsOwner)
            SetAnimSpeedServerRpc(_agent.velocity.magnitude);
    }

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
        CustomerSetup();
        _totalWaitTime = GetItems.GetRecipeByDishType(requestedDish.Value).cookingMinMax.y + 90f;
        _elapsedTime = _totalWaitTime;

        if (!IsServer) return;

        _waypoints.Add(GameObject.FindGameObjectWithTag("Ordering").transform);
        _waypoints.Add(GameObject.FindGameObjectWithTag("Entrance").transform);
        _waypoints.Add(GameObject.FindGameObjectWithTag("Waiting").transform);
        
        _blackboardReference = _behaviorGraphAgent.BlackboardReference;
        _blackboardReference.SetVariableValue("Customer", this);

        // Assign waypoints
        _blackboardReference.SetVariableValue("OrderingLocation", _waypoints[0]);
        _blackboardReference.SetVariableValue("LeaveLocation", _waypoints[1]);
        _blackboardReference.SetVariableValue("WaitingLocation", _waypoints[2]);
        _blackboardReference.SetVariableValue("WaitingTime", _totalWaitTime);

        // Add to waiting line
        _blackboardReference.GetVariableValue("CustomersInLine", out List<GameObject> customerList);
        if (!customerList.Contains(gameObject)) customerList.Add(gameObject);
        _blackboardReference.SetVariableValue("CustomersInLine", customerList);
    }

    private void CustomerSetup()
    {
        ChangeFoodIcon(requestedDish.Value);
        Utilis.ShowSelectedMesh(ears, selectedEarsIndex.Value);
        Utilis.ShowSelectedMesh(shirtMesh, selectedShirtIndex.Value);
        Utilis.ShowSelectedMesh(pantsMesh, selectedPantsIndex.Value);
        Utilis.ShowSelectedMesh(hairMesh, selectedHairIndex.Value);

        faceRenderer.sharedMaterials = new[]{ new Material (faces[selectedFaceIndex.Value]) };
        foreach (var mesh in customerMesh) {
            mesh.sharedMaterials = new[] { skins[selectedSkinIndex.Value] };
        }
        
        hairMesh[selectedHairIndex.Value].sharedMaterial = hairMats[selectedHairMat.Value];
        shirtMesh[selectedShirtIndex.Value].sharedMaterial = clothesMats[selectedShirtMat.Value];
        pantsMesh[selectedPantsIndex.Value].sharedMaterial = clothesMats[selectedPantsMat.Value];
    }

    private void ChangeFoodIcon(DishType newValue)
    {
        foodIcon.sprite = Resources.Load<Sprite>("Icons/Food/" + newValue);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _messageAwaitable?.Cancel();
        _timerAwaitable?.Cancel();
        if (!IsServer) return;
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

    public IInteractable PickupOrDropObject(bool pickUp,
        Vector3 placePosition)
    {
        return null;
    }

    public IInteractable SecondaryInteract(OwnerPlayer interactor)
    {
        TakeOrderRpc();
        return null;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void TakeOrderRpc()
    {
        _blackboardReference.SetVariableValue("Ordered", true);
        _aiManager.AddDish(requestedDish.Value);
        orderTaken.Value = true;
    }

    [Rpc(SendTo.Everyone)]
    public void ShowMessageRpc(FixedString128Bytes message)
    {
        _messageAwaitable = ShowMessage(message);
    }
    
    public async Awaitable ShowMessage(FixedString128Bytes message)
    {
        messageText.text = message.ToString();
        await Awaitable.WaitForSecondsAsync(3);
        messageText.text = "";
    }
    
    [Rpc(SendTo.Everyone)]
    public void StartTimerRpc()
    {
        _timerAwaitable = StartTimer();
    }

    private async Awaitable StartTimer()
    { 
        while(timeImage.fillAmount > 0f)
        {
            _elapsedTime -= Time.deltaTime;
            var fAmount = _elapsedTime / _totalWaitTime;
            var c = new Color(1 - fAmount, fAmount, 0);
            timeImage.fillAmount = fAmount;
            timeImage.color = c;
            await Awaitable.EndOfFrameAsync();
        }
    }


    public void RemoveDish()
    {
        _aiManager.RemoveDish(requestedDish.Value);
        RemoveDishRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void RemoveDishRpc()
    {
        _timerAwaitable?.Cancel();
        timeImage.enabled = false;
        foodIcon.enabled = false;
    }

    public string GetInteractText()
    {
        return $"Take order from\n{customerName.Value}";
    }

    public bool IsInteractedWith()
    {
        return orderTaken.Value;
    }

    public void ChangeOutline(bool show)
    {
        throw new NotImplementedException();
    }

    #endregion

}

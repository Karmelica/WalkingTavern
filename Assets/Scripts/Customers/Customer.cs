using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cooking.ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Customer : NetworkBehaviour, IInteractable
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform lineTransform;
    [SerializeField] private bool isMoving = true;

    public CustomerState state { get; private set; } = CustomerState.WaitingInLine;
    public bool HasSeat = false;
    public bool DespawnAfterArriving;
    [SerializeField] private Transform myDestination;
    private Recipe _requestedRecipe;
    private AIManager _aiManager;

    #region Unity Lifecycle

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn(); 
        myDestination = transform;
    }

    private void Update()
    {
        Gravity();
        agent.SetDestination(myDestination.position);
        if (agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, myDestination.rotation, Time.deltaTime * 5f);
            if (DespawnAfterArriving && Vector3.Distance(transform.position, myDestination.position) < 1f)
            {
                _aiManager.DespawnCustomer(this);
            }
        }
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

    private void GoToNextState()
    {
        if (!IsServer) return;
        if(state == CustomerState.Leaving) return;
        
        SetState(state + 1);
    }
    
    public void SetState(CustomerState newState)
    {
        if (newState == CustomerState.WaitingForFood)
        {
            StartCoroutine(WaitThreshold(30f));
        }
        state = newState;
        _aiManager.CheckState();
    }

    private IEnumerator WaitThreshold(float time)
    {
        yield return new WaitForSeconds(time);
        SetState(CustomerState.Leaving);
    }

    public void SetManager(AIManager manager)
    {
        _aiManager = manager;
    }
    
    public void SetDestination(Transform destination)
    {
        myDestination = destination;
    }
    public Transform GetDestination()
    {
        return myDestination;
    }
    
    public void SetRecipe(Recipe newRecipe)
    {
        _requestedRecipe = newRecipe;
    }
    
    public Transform GetLineTransform()
    {
        return lineTransform;
    }

    #endregion

    #region Interact

    public void PrimaryInteract(NetworkBehaviourReference interactor, bool pickingUp = true)
    {
        //nothing
    }

    public void SecondaryInteract(NetworkBehaviourReference interactor)
    {
        GoToNextState();
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

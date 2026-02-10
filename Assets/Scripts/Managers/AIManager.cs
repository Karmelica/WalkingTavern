using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cooking.ScriptableObjects;
using Unity.Netcode;
using UnityEngine;

public class AIManager : NetworkBehaviour
{

    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private List<Transform> takenSeats;
    [SerializeField] private Transform spawnPoint;
    private List<Customer> _customers = new();
    private Customer _orderingCustomer;
    private Customer _customerInFront;
    
    private Queue<Transform> _availableSeats = new();
    
    private List<Recipe> _availableRecipes = new ();

    private void Start()
    {
        LoadRecipes();
        
        foreach (var seat in takenSeats.ToList())
        {
            _availableSeats.Enqueue(seat);
            takenSeats.Remove(seat);
        }

        for(var i = 0; i < 8; i++){
            SpawnCustomer();
        }
        
        CheckState();
    }
    
    private void LoadRecipes()
    {
        var recipes = Resources.LoadAll<Recipe>("ScriptableObjects/Cooking");
        _availableRecipes.AddRange(recipes);
    }

    public void CheckState()
    {
        if (!IsServer) return;
        foreach (var customer in _customers)
        {
            switch (customer.state)
            {
                case CustomerState.WaitingInLine:
                    WaitingInLineState(customer);
                    break;
                case CustomerState.Ordering:
                    OrderingState(customer);
                    break;
                case CustomerState.WaitingForFood:
                    WaitingForFoodState(customer);
                    break;
                case CustomerState.Eating:
                    EatingState(customer);
                    break;
                case CustomerState.Leaving:
                    LeavingState(customer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        _customerInFront = null;
    }

    private void WaitingInLineState(Customer customer)
    {
        if (!_orderingCustomer)
        {
            _orderingCustomer = customer;
            GoToNextState(customer);
        }
        else if (!_customerInFront)
        {
            customer.SetDestination(_orderingCustomer.GetLineTransform());
            _customerInFront = customer;
        }
        else
        {
            if(customer == _customerInFront) return;
            customer.SetDestination(_customerInFront.GetLineTransform());
            _customerInFront = customer;
        }
    }

    private void OrderingState(Customer customer)
    {
        if(customer == _orderingCustomer)
        {
            customer.SetDestination(waypoints[0]);
        }
    }

    private void WaitingForFoodState(Customer customer)
    {
        if(_orderingCustomer == customer)
        {
            _orderingCustomer = null;
        }

        if (customer.HasSeat) return;
        if(_availableSeats.Count > 0){
            customer.HasSeat = true;
            var seat = _availableSeats.Dequeue();
            takenSeats.Add(seat);
            customer.SetDestination(seat);
        }
        else
        {
            customer.SetState(CustomerState.Leaving);
        }
    }

    private void EatingState(Customer customer)
    {
        //customer.SetDestination(waypoints[1]);
    }

    private void LeavingState(Customer customer)
    {
        if (customer.HasSeat)
        {
            customer.HasSeat = false;
            takenSeats.Remove(customer.GetDestination());
            _availableSeats.Enqueue(customer.GetDestination());
        }
        customer.SetDestination(waypoints[2]);
        customer.DespawnAfterArriving = true;
    }
    
    private void GoToNextState(Customer customer)
    {
        if (!IsServer) return;
        if(customer.state == CustomerState.Leaving) return;
        
        customer.SetState(customer.state + 1);
    }

    private void SpawnCustomer()
    {
        if (!IsServer) return;
        var customerInstance = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        var customer = customerInstance.GetComponent<Customer>();
        customer.SetRecipe(_availableRecipes[UnityEngine.Random.Range(0, _availableRecipes.Count)]);
        customer.SetManager(this);
        _customers.Add(customer);
        customerInstance.GetComponent<NetworkObject>().Spawn();
    }
    
    public void DespawnCustomer(Customer customer)
    {
        if (!IsServer) return;
        _customers.Remove(customer);
        customer.NetworkObject.Despawn();

    }
}

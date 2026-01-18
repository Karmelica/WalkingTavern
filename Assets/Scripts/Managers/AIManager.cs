using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AIManager : NetworkBehaviour
{
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private Transform spawnPoint;
    private List<Customer> _customers = new();
    private Customer _orderingCustomer;
    private Customer _customerInFront;
    

    private void Start()
    {
        SpawnCustomer();
        SpawnCustomer();
        SpawnCustomer();

        if (IsServer)
        {
            StartCoroutine(AIManagement());
        }
    }

    private IEnumerator AIManagement()
    {
        while (true)
        {
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
            yield return new WaitForSeconds(0.1f);
        }
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
        customer.SetDestination(waypoints[1]);
    }

    private void EatingState(Customer customer)
    {
        customer.SetDestination(waypoints[1]);
    }

    private void LeavingState(Customer customer)
    {
        customer.SetDestination(waypoints[2]);
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
        _customers.Add(customerInstance.GetComponent<Customer>());
        customerInstance.GetComponent<NetworkObject>().Spawn();
    }
}

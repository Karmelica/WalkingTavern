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
    private List<Customer> _lineCustomers = new();
    private Customer _orderingCustomer;
    private Queue<Customer> _waitingCustomers = new();
    private Queue<Customer> _eatingCustomers = new();
    

    private void Start()
    {
        SpawnCustomer();
        SpawnCustomer();
        SpawnCustomer();

        if (IsServer)
        {
            StartCoroutine(AIManagement());
            StartCoroutine(ByeBye());
        }
    }

    private IEnumerator ByeBye()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);
            GoToNextState();
        }
    }

    private IEnumerator AIManagement()
    {
        while (true)
        {
            if (!_orderingCustomer && _lineCustomers.Count > 0)
            {
                var orderingCustomer = _lineCustomers[0];
                _lineCustomers.RemoveAt(0);
                _orderingCustomer = orderingCustomer;
                _orderingCustomer.SetState(CustomerState.Ordering);
                _orderingCustomer.SetDestination(waypoints[0]);
            }

            if (_lineCustomers.Count > 0)
            {
                _lineCustomers[0].SetState(CustomerState.WaitingInLine);
                _lineCustomers[0].SetDestination(_orderingCustomer.transform);
            }
            if (_lineCustomers.Count > 1)
            {
                for (int i = 1; i < _lineCustomers.Count; i++)
                {
                    _lineCustomers[i].SetState(CustomerState.WaitingInLine);
                    _lineCustomers[i].SetDestination(_lineCustomers[i - 1].transform);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void SpawnCustomer()
    {
        if (!IsServer) return;
        var customerInstance = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        _lineCustomers.Add(customerInstance.GetComponent<Customer>());
        customerInstance.GetComponent<NetworkObject>().Spawn();
    }
    
    public void GoToNextState()
    {
        if (!IsServer) return;
        if(_orderingCustomer){
            _orderingCustomer.SetState(CustomerState.Eating);
            _orderingCustomer.SetDestination(waypoints[2]);
            _eatingCustomers.Enqueue(_orderingCustomer);
            _orderingCustomer = null;
        }
        /*foreach (var eatingCustomer in _eatingCustomers)
        {
            eatingCustomer.SetState(CustomerState.Leaving);
            eatingCustomer.SetDestination(waypoints[0]);
        }*/
    }
}

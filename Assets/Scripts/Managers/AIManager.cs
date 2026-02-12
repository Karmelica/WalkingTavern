using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cooking.ScriptableObjects;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class AIManager : NetworkBehaviour
{

    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private List<Transform> waypoints;
    [SerializeField] private List<Transform> seats;
    [SerializeField] private Transform spawnPoint;
    private List<Customer> _customers = new();
    private Customer _orderingCustomer;
    private Customer _customerInFront;
    
    private Queue<Transform> _availableSeats = new();
    
    private List<Recipe> _availableRecipes = new ();

    private void Start()
    {
        LoadRecipes();
        
        foreach (var seat in seats.ToList())
        {
            _availableSeats.Enqueue(seat);
            seats.Remove(seat);
        }

        for(var i = 0; i < 8; i++){
            SpawnCustomer();
        }
    }
    
    private void LoadRecipes()
    {
        var recipes = Resources.LoadAll<Recipe>("ScriptableObjects/Cooking");
        _availableRecipes.AddRange(recipes);
    }
    
    public Transform TryGetAvailableSeat()
    {
        return _availableSeats.Count > 0 ? _availableSeats.Dequeue() : null;
    }
    
    public void ReturnSeat(Transform seat)
    {
        _availableSeats.Enqueue(seat);
    }
    
    private void SpawnCustomer()
    {
        if (!IsServer) return;
        var customerInstance = Instantiate(customerPrefab, spawnPoint.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2)), Quaternion.identity);
        var customer = customerInstance.GetComponent<Customer>();
        var recipe = _availableRecipes[Random.Range(0, _availableRecipes.Count)];
        customer.AssignVariables(this, recipe, waypoints);
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

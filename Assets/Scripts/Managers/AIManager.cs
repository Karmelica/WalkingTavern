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
    [SerializeField] private int customersToSpawn;

    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private Transform spawnPoint;
    private Customer _orderingCustomer;
    private Customer _customerInFront;
    
    private Queue<Transform> _availableSeats = new();
    
    private List<Recipe> _availableRecipes = new ();

    private void Start()
    {
        LoadRecipes();

        var worldSeats = GameObject.FindGameObjectsWithTag("Seat");
        
        foreach (var worldSeat in worldSeats)
        {
            var seat = worldSeat.transform;
            _availableSeats.Enqueue(seat);
        }

        for(var i = 0; i < customersToSpawn; i++){
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
    
    public void SpawnCustomer()
    {
        if (!IsServer) return;
        var customerInstance = Instantiate(customerPrefab, spawnPoint.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
        var customer = customerInstance.GetComponent<Customer>();
        customer.AssignVariables(this);
        
        customer.requestedDish = _availableRecipes[Random.Range(0, _availableRecipes.Count)].dishType; 
        int rand = Random.Range(0, customer.ears.Count);
        customer.ears[rand].SetActive(true);
        
        customerInstance.GetComponent<NetworkObject>().Spawn();
    }
    
    public void DespawnCustomer(Customer customer)
    {
        if (!IsServer) return;
        customer.NetworkObject.Despawn();
        SpawnCustomer();
    }
}

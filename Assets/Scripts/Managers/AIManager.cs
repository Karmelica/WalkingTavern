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
    [SerializeField] private int totalCustomers = 12;
    [SerializeField] private float spawnTime = 30f;

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

        StartCoroutine(SpawnCustomer());
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
    
    public IEnumerator SpawnCustomer()
    {
        if (!IsServer) yield return null;
        for(int i = 0; i < totalCustomers; i++)
        {
            var customerInstance = Instantiate(customerPrefab,
                spawnPoint.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)), Quaternion.identity);
            var customer = customerInstance.GetComponent<Customer>();
            customer.AssignVariables(this);

            var randomDish = _availableRecipes[Random.Range(0, _availableRecipes.Count)].dishType;
            customer.requestedDish = new NetworkVariable<DishType>(randomDish);

            int rand = Random.Range(0, customer.ears.Count);
            customer.selectedEarsIndex = new NetworkVariable<int>(rand);
            rand = Random.Range(0, customer.skins.Count);
            customer.selectedSkinIndex = new NetworkVariable<int>(rand);
            rand = Random.Range(0, customer.faces.Count);
            customer.selectedFaceIndex = new NetworkVariable<int>(rand);

            customer.customerName = new();

            customerInstance.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForSeconds(spawnTime);
        }
    }
    
    public void DespawnCustomer(Customer customer)
    {
        if (!IsServer) return;
        customer.NetworkObject.Despawn();
        SpawnCustomer();
    }
}

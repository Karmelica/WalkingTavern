using System;
using System.Collections;
using System.Collections.Generic;
using Cooking.ScriptableObjects;
using PlayerScripts;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class AIManager : NetworkBehaviour
    {
        public static Action<int> OnScoreChanged;
        public static Action<DishType> OnDishChanged;
    
        [SerializeField] private int totalScore;
        [SerializeField] private int streak;
        [SerializeField] private int totalCustomers = 12;
        [SerializeField] private float spawnTime = 30f;
        private Dictionary<DishType, int> _dishes = new();

        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform spawnPoint;
        private Customer _orderingCustomer;
        private Customer _customerInFront;
    
        private Queue<Transform> _availableSeats = new();
    
        private List<Recipe> _availableRecipes = new ();

        private void OnEnable()
        {
            LoadRecipes();
            
            var worldSeats = GameObject.FindGameObjectsWithTag("Seat");

            foreach (var worldSeat in worldSeats)
            {
                var seat = worldSeat.transform;
                _availableSeats.Enqueue(seat);
            }
            
            OnScoreChanged += UpdateScore;
        }

        private void OnDisable()
        {
            OnScoreChanged -= UpdateScore;
        }

        private void UpdateScore(int scoreChange)
        {
            totalScore = Math.Max(0, (totalScore + scoreChange) * streak);
            streak = scoreChange > 0 ? streak + 1 : 1;
            UpdateGUI();
        }

        public void AddDish(DishType dish)
        {
            if (!_dishes.TryAdd(dish, 1))
            {
                _dishes[dish]++;
            }
            UpdateGUI();
        }
        
        public void RemoveDish(DishType dish)
        {
            if(_dishes.ContainsKey(dish))
            {
                _dishes[dish]--;
                if (_dishes[dish] <= 0)
                {
                    _dishes.Remove(dish);
                }
            }
            UpdateGUI();
        }

        private void UpdateGUI()
        {
            string info = "";
            info += $"Score: {totalScore}";
            info += $"\nRequested Dishes:";
            foreach (var dish in _dishes)
            {
                info += $"\n  {dish.Key.ToString()} x{dish.Value.ToString()}";
            }
            SendUpdatedScoreRpc(info);
        }

        [Rpc(SendTo.Everyone)]
        private void SendUpdatedScoreRpc(FixedString512Bytes info)
        {
            PlayerGUI.OnGameInfoChanged?.Invoke(info);
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

        public void StartSpawningCustomers() => StartCoroutine(SpawnCustomer());

        private IEnumerator SpawnCustomer()
        {
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
        }
    }
}

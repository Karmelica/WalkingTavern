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
    
        [SerializeField] private int totalScore;
        [SerializeField] private float streak;
        [SerializeField] private int totalCustomers = 12;
        [SerializeField] private float spawnTime = 30f;
        private Dictionary<DishType, int> _dishes = new();

        
        [SerializeField] private List<string> firstNames = new();
        [SerializeField] private List<string> secondNames = new();
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform spawnPoint;
        private Customer _orderingCustomer;
        private Customer _customerInFront;
    
        private Queue<Transform> _availableSeats = new();
    
        private List<Recipe> _availableRecipes = new ();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            UpdateScore(0);
        }

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

        private void UpdateScore(int scoreChange)
        {
            if(scoreChange > 0){
                totalScore = Mathf.RoundToInt(totalScore + (scoreChange * streak));
                streak += 0.5f;
                PlayKachingSoundRpc();
            } else {
                totalScore = Mathf.RoundToInt(Math.Max(0, totalScore + scoreChange));
                streak = 1f;
            }
            streak = Mathf.Min(streak, 5.5f);
            UpdateGUI();
        }

        
        [Rpc(SendTo.Everyone)]
        private void PlayKachingSoundRpc()
        {
            AudioManager.Instance.PlayOneShot(AudioEvents.Instance.money);
        }

        private void UpdateGUI()
        {
            string info = "";
            info += $"Score: {totalScore}";
            info += "\nRequested Dishes:";
            if (_dishes.Count != 0) {
                foreach (var dish in _dishes) {
                    info += $"\n  {dish.Key.ToString()} x{dish.Value.ToString()}";
                }
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

                int rand = Random.Range(0, customer.ears.Length);
                customer.selectedEarsIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.skins.Count);
                customer.selectedSkinIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.faces.Count);
                customer.selectedFaceIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.hairMesh.Length);
                customer.selectedHairIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.pantsMesh.Length);
                customer.selectedShirtIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.shirtMesh.Length);
                customer.selectedPantsIndex = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.clothesMats.Count);
                customer.selectedShirtMat = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.clothesMats.Count);
                customer.selectedPantsMat = new NetworkVariable<int>(rand);
                rand = Random.Range(0, customer.hairMats.Count);
                customer.selectedHairMat = new NetworkVariable<int>(rand);

                customer.customerName = new(GenerateName());

                customerInstance.GetComponent<NetworkObject>().Spawn();
                yield return new WaitForSeconds(spawnTime);
            }
        }

        private string GenerateName()
        {
            if (firstNames.Count == 0 || secondNames.Count == 0)
            {
                return "Noname";
            }
            return $"{firstNames?[Random.Range(0, firstNames.Count)]} {secondNames?[Random.Range(0, secondNames.Count)]}";
        }
    }
}

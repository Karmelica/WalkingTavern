using PlayerScripts;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace World.Caravan
{
    public class StorageBox : NetworkBehaviour, IInteractable
    {
        [SerializeField] private IngredientType ingredientBox;
        [SerializeField] private Image foodIcon;
        private NetworkVariable<int> _quantity = new();
        private int _localQuantity;

        private void Start()
        {
            foodIcon.material = new Material(Resources.Load<Material>("Icons/Food/FoodIcon"))
            {
                mainTexture = Resources.Load<Texture>("Icons/Food/" + ingredientBox)
            };
        }

        protected override void OnNetworkPostSpawn()
        {
            base.OnNetworkPostSpawn();
            _quantity.OnValueChanged += OnQuantityChanged;
            if(IsServer && FoodStorage.Instance) _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _quantity.OnValueChanged -= OnQuantityChanged;
        }

        private void OnQuantityChanged(int previousValue, int newValue)
        {
            _localQuantity = newValue;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsServer) return;
            if(other.TryGetComponent(out FoodItem foodItem) && !foodItem.IsInteractedWith() && foodItem.ingredientType == ingredientBox)
            {
                FoodStorage.Instance.ReturnIngredient(ingredientBox);
                foodItem.NetworkObject.Despawn();
                _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox);
            }
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            SpawnIngredientServerRpc(ingredientBox);
            return null;
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SpawnIngredientServerRpc(IngredientType ingredientType)
        {
            if (!IsServer) return;
            if (!FoodStorage.Instance) return;
            if (!FoodStorage.Instance.GetIngredient(ingredientType)) return;
            var ingredient = Instantiate(Resources.Load<GameObject>("Prefabs/Food/Ingredients/" + ingredientType), transform.position + transform.forward, Quaternion.identity);
            ingredient.GetComponent<NetworkObject>().Spawn();
            _quantity.Value = FoodStorage.Instance.GetIngredientCount(ingredientBox);
        }

        public string GetInteractName()
        {
            return "\nStorage (" + ingredientBox + ": " + _localQuantity + ")";
        }

        public bool IsInteractedWith()
        {
            return false;
        }
    }
}

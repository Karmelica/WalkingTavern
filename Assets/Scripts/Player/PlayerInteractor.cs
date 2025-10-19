using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public interface IInteractable
    {
        public void PrimaryInteract(ref GameObject heldObject);
    }

    public class PlayerInteractor : NetworkBehaviour
    {
        [SerializeField] private float interactRange = 3f;
        private Camera _mainCamera;
    
        private InputSystem_Actions _inputActions;
        private InputSystem_Actions.PlayerActions _playerActions;
    
        [SerializeField] private GameObject heldObject;
    
        public override void OnNetworkSpawn()
        {
            // Sprawdź, czy jesteśmy właścicielem
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
        
            _inputActions = new InputSystem_Actions();
            _playerActions = _inputActions.Player;
            _mainCamera = Camera.main;
            _playerActions.Enable();
            _playerActions.Interact.started += Interact;
        
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            _playerActions.Interact.started -= Interact;
            _playerActions.Disable();
        }

        private void Interact(InputAction.CallbackContext obj)
        {
            var interactPoint = _mainCamera.transform;
            var ray = new Ray(interactPoint.position, interactPoint.forward);
            if (!Physics.Raycast(ray, out var hitInfo, interactRange)) return;
            if (heldObject == null)
            {
                if (hitInfo.collider.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.PrimaryInteract(ref heldObject);

                }
            }
            else
            {
                heldObject.transform.position = hitInfo.point + Vector3.up * 0.5f;
                heldObject.SetActive(true);
                heldObject.GetComponent<NetworkObject>().Spawn();
                heldObject = null;
            }
        }
    }
}
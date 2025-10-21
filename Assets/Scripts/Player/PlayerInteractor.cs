using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public interface IInteractable
    {
        public void PrimaryInteract(ref GameObject heldObject);
    }

    public class PlayerInteractor : NetworkBehaviour, InputSystem_Actions.IPlayerActions
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
            
            StartCoroutine(WaitForMainCamera());
            _inputActions = new InputSystem_Actions();
            _playerActions = _inputActions.Player;
            _playerActions.SetCallbacks(this);
            _playerActions.Enable();
        
        }
        
        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;
            _playerActions.Disable();
            _inputActions.Dispose();
        }
        
        private IEnumerator WaitForMainCamera()
        {
            yield return new WaitUntil(() => Camera.main);
            _mainCamera = Camera.main;
        }
        
        public void OnInteract(InputAction.CallbackContext context)
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

        public void OnMove(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            //TODO
        }


        public void OnCrouch(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            //TODO
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            //TODO
        }
    }
}
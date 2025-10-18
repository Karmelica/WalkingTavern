using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public interface IInteractable
{
    public void PrimaryInteract(Transform arm);
}

public class Interactor : NetworkBehaviour
{
    [SerializeField] private float interactRange = 3f;
    private Camera _mainCamera;
    
    private InputSystem_Actions _inputActions;
    private InputSystem_Actions.PlayerActions _playerActions;
    
    public override void OnNetworkSpawn()
    {
        // Sprawdź czy jesteśmy właścicielem
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
        _playerActions.Interact.performed += Interact;
        
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            _playerActions.Interact.started -= Interact;
            _playerActions.Interact.performed -= Interact;
            _playerActions.Disable();
        }
    }

    private void Interact(InputAction.CallbackContext obj)
    {
        var interactPoint = _mainCamera.transform;
        var ray = new Ray(interactPoint.position, interactPoint.forward);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, interactRange)) return;
        if(hitInfo.collider.TryGetComponent(out IInteractable interactObj))
        {
            interactObj.PrimaryInteract(_mainCamera.transform);
        }
    }
}

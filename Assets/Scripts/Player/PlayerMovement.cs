using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    
    public class PlayerMovement : MonoBehaviour
    {
        private InputSystem_Actions _inputActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        [Header("Components")]
        private Rigidbody _rb;
        private Collider _colli;
        private Camera _playerCamera;
        
        [Header("Private variables")]
        private Vector2 _inputVector;
        private bool isGrounded;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _colli = GetComponent<Collider>();
            _playerCamera = Camera.main;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 moveVector = _inputVector.y * transform.forward + _inputVector.x * transform.right;
            
            _rb.AddForce(moveVector * 10, ForceMode.Force);
        }

        private void Update()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, _colli.bounds.extents.y + 0.1f);
            Debug.DrawRay(transform.position, Vector3.down * (_colli.bounds.extents.y + 0.1f), Color.red);
            if(_playerCamera)
            {
                _playerCamera.transform.position = transform.position + new Vector3(0, 1f, 0);
                _playerCamera.transform.rotation = transform.rotation;
            }
            
        }

        private void OnEnable()
        {
            _inputActions = new InputSystem_Actions();
            _playerActions = _inputActions.Player;
            _playerActions.Enable();
            _playerActions.Jump.performed += JumpInput;
            _playerActions.Jump.canceled += JumpInput;
            _playerActions.Move.performed += MoveInput;
            _playerActions.Move.canceled += MoveInput;
            _playerActions.Look.performed += LookInput;
        }
        
        private void OnDisable()
        {
            _playerActions.Jump.performed -= JumpInput;
            _playerActions.Jump.canceled -= JumpInput;
            _playerActions.Move.performed -= MoveInput;
            _playerActions.Move.canceled -= MoveInput;
            _playerActions.Look.performed -= LookInput;
            _playerActions.Disable();
        }

        private void Jump()
        {
            if(isGrounded){
                _rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
            }
        }
        
        private void LookInput(InputAction.CallbackContext context)
        {
            Vector2 lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0, lookVector.x * 0.1f, 0);
            if(_playerCamera)
            {
                _playerCamera.transform.Rotate(-lookVector.y * 0.1f, 0, 0);
            }
        }
        
        private void JumpInput(InputAction.CallbackContext context)
        {
                if(context.performed)
                    InvokeRepeating(nameof(Jump), 0, 0.1f);
                if(context.canceled)
                    CancelInvoke(nameof(Jump));
        }
        
        private void MoveInput(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
        }
    }
}

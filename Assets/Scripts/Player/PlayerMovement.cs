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
        
        private Rigidbody _rb;
        private Collider _colli;
        
        private Vector2 _inputVector;
        [SerializeField] private bool isGrounded;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _colli = GetComponent<Collider>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector3 moveVector = new Vector3(_inputVector.x, 0, _inputVector.y);
            _rb.AddForce(moveVector * 10, ForceMode.Force);
        }

        private void Update()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, _colli.bounds.extents.y + 0.1f);
            Debug.DrawRay(transform.position, Vector3.down * (_colli.bounds.extents.y + 0.1f), Color.red);
        }

        private void OnEnable()
        {
            _inputActions = new InputSystem_Actions();
            _playerActions = _inputActions.Player;
            _playerActions.Enable();
            _playerActions.Jump.performed += JumpInput;
            _playerActions.Jump.canceled += JumpInput;
            _playerActions.Move.performed += Move;
            _playerActions.Move.canceled += Move;
        }
        
        private void OnDisable()
        {
            _playerActions.Jump.performed -= JumpInput;
            _playerActions.Jump.canceled -= JumpInput;
            _playerActions.Move.performed -= Move;
            _playerActions.Move.canceled -= Move;
            _playerActions.Disable();
        }

        private void Jump()
        {
            if(isGrounded){
                _rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
            }
        }
        
        private void JumpInput(InputAction.CallbackContext context)
        {
                Debug.Log("Jump");
                if(context.performed)
                    InvokeRepeating(nameof(Jump), 0, 0.1f);
                if(context.canceled)
                    CancelInvoke(nameof(Jump));
        }
        
        private void Move(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
        }
    }
}

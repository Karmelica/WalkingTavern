using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private List<GameObject> customizations = new List<GameObject>();
        
        private InputSystem_Actions _inputActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        [Header("Components")]
        private Rigidbody _rb;
        private Collider _colli;
        private Camera _playerCamera;
        
        [Header("Private variables")]
        private Vector2 _inputVector;
        private bool isGrounded;
        private bool isSprinting;

        #region Network

        public override void OnNetworkSpawn()
        {
            _playerCamera = Camera.main;
            if (IsOwner)
            {
                foreach (var customization in customizations)
                {
                    customization.SetActive(false);
                }
            }
        }

        #endregion

        #region Unity

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _colli = GetComponent<Collider>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if(IsOwner) Move();
        }

        private void Update()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, _colli.bounds.extents.y + 0.1f);
            Debug.DrawRay(transform.position, Vector3.down * (_colli.bounds.extents.y + 0.1f), Color.red);
            if (_playerCamera && IsOwner)
            {
                _playerCamera.transform.position = transform.position + new Vector3(0, 1f, 0);
                _playerCamera.transform.rotation = Quaternion.LookRotation(transform.forward);
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
            _playerActions.Sprint.performed += SprintInput;
            _playerActions.Sprint.canceled += SprintInput;
        }
        
        private void OnDisable()
        {
            _playerActions.Jump.performed -= JumpInput;
            _playerActions.Jump.canceled -= JumpInput;
            _playerActions.Move.performed -= MoveInput;
            _playerActions.Move.canceled -= MoveInput;
            _playerActions.Look.performed -= LookInput;
            _playerActions.Sprint.performed -= SprintInput;
            _playerActions.Sprint.canceled -= SprintInput;
            _playerActions.Disable();
        }

        #endregion
        
        private void Jump()
        {
            if(isGrounded){
                _rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
            }
        }
        
        private void Move()
        {
            Vector3 moveVector = _inputVector.y * transform.forward + _inputVector.x * transform.right;
            _rb.AddForce(isSprinting? moveVector * 20 : moveVector * 10, ForceMode.Force);
        }

        #region Inputs

        private void LookInput(InputAction.CallbackContext context)
        {
            if(!Application.isFocused || !IsOwner) return;
            Vector2 lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0, lookVector.x * 0.1f, 0);
        }
        
        private void JumpInput(InputAction.CallbackContext context)
        {
            if(IsOwner){
                if (context.performed)
                    InvokeRepeating(nameof(Jump), 0, 0.1f);
                if (context.canceled)
                    CancelInvoke(nameof(Jump));
            }
        }
        
        private void MoveInput(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
        }
        
        private void SprintInput(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                isSprinting = true;
            }
            if (context.canceled)
            {
                isSprinting = false;
            }
        }

        #endregion
    }
}

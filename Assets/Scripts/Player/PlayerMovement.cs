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
        private Collider _coli;
        private Camera _playerCamera;
        
        [Header("Private variables")]
        private Vector2 _inputVector;
        private bool _isGrounded;
        private bool _isSprinting;

        #region Network

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _playerCamera = Camera.main;
                foreach (var customization in customizations)
                {
                    customization.SetActive(false);
                }
                _rb = GetComponent<Rigidbody>();
                _coli = GetComponent<Collider>();
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #endregion

        #region Unity

        // Update is called once per frame
        void FixedUpdate()
        {
            if(IsOwner) Move();
        }

        private void Update()
        {
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, _coli.bounds.extents.y + 0.1f);
            //Debug.DrawRay(transform.position, Vector3.down * (_coli.bounds.extents.y + 0.1f), Color.red);
            if (_playerCamera && IsOwner)
            {
                _playerCamera.transform.position = transform.position + new Vector3(0, 0.5f, 0);
                /*var lookVectorY = _playerCamera.transform.rotation.eulerAngles.x > 180f
                    ? _playerCamera.transform.rotation.eulerAngles.x - 360f
                    : _playerCamera.transform.rotation.eulerAngles.x;*/
                var lookVectorY = Mathf.Clamp(NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x), -87f, 87f);
                _playerCamera.transform.localRotation = Quaternion.Euler(new Vector3(lookVectorY, transform.localRotation.eulerAngles.y, 0));
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
            if(_isGrounded){
                _rb.AddForce(Vector3.up * 7, ForceMode.Impulse);
            }
        }
        
        private void Move()
        {
            Vector3 moveVector = _inputVector.y * transform.forward + _inputVector.x * transform.right;
            _rb.AddForce(_isSprinting? moveVector * 20 : moveVector * 10, ForceMode.Force);
        }
        
        private float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        #region Inputs

        private void LookInput(InputAction.CallbackContext context)
        {
            if(!Application.isFocused || !IsOwner) return;
            Vector2 lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0, lookVector.x * 0.1f, 0);
            _playerCamera.transform.Rotate(-lookVector.y * 0.1f, 0, 0);
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
                _isSprinting = true;
            }
            if (context.canceled)
            {
                _isSprinting = false;
            }
        }

        #endregion
    }
}

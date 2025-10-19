using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(NetworkAnimator))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkRigidbody))]
    [RequireComponent(typeof(Collider))]
    
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Animator Parameters")]
        private readonly NetworkVariable<float> _networkWalkSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<float> _networkYSpeed = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _networkIsGrounded = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int VelocityY = Animator.StringToHash("VelocityY");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        
        
        [SerializeField] private SkinnedMeshRenderer customization;
        
        private InputSystem_Actions _inputActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        [Header("Components")]
        private Animator _animator;
        private Rigidbody _rb;
        //private Collider _coli;
        private Camera _playerCamera;
        
        [Header("Private variables")]
        private Vector2 _inputVector;
        private bool _isGrounded;
        private bool _isSprinting;

        #region Network

        public override void OnNetworkSpawn()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();
            switch (IsOwner)
            {
                case false:
                    _networkWalkSpeed.OnValueChanged += (float previousValue, float newValue) =>
                    {
                        _animator.SetFloat(WalkSpeed, newValue);
                    };
                    _networkYSpeed.OnValueChanged += (float previousValue, float newValue) =>
                    {                    
                        _animator.SetFloat(VelocityY, newValue);
                    };
                    _networkIsGrounded.OnValueChanged += (bool previousValue, bool newValue) =>
                    {
                        _animator.SetBool(IsGrounded, newValue);
                    };
                    break;
                case true:
                    _playerCamera = Camera.main;
                    //customization.enabled = false;
                    break;
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
            if (IsOwner)
            {
                _networkWalkSpeed.Value = _rb.linearVelocity.magnitude;
                _networkYSpeed.Value = Mathf.FloorToInt(_rb.linearVelocity.y);
            }
            _animator.SetFloat(WalkSpeed, _rb.linearVelocity.magnitude);
            _animator.SetFloat(VelocityY, Mathf.FloorToInt(_rb.linearVelocity.y));
            _isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
            _animator.SetBool(IsGrounded, _isGrounded);
            if (!IsOwner) return;
            //Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.red);
            if (!_playerCamera) return;
            _playerCamera.transform.position = transform.position + new Vector3(0, 1.7f, 0);
            var lookVectorY = Mathf.Clamp(NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x), -87f,
                87f);
            _playerCamera.transform.localRotation =
                Quaternion.Euler(new Vector3(lookVectorY, transform.localRotation.eulerAngles.y, 0));
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
                _rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
            }
        }
        private void Move()
        {
            var moveVector = _inputVector.y * transform.forward + _inputVector.x * transform.right;
            _rb.AddForce(_isSprinting ? moveVector * 20f : moveVector * 10f, ForceMode.Force);
            if (!(_rb.linearVelocity.magnitude > (_isSprinting ? 10f : 5f))) return;
            Vector2 limitedVelocity = new(_rb.linearVelocity.x, _rb.linearVelocity.z);
            limitedVelocity = limitedVelocity.normalized * (_isSprinting ? 10f : 5f);
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.y);

        }
        
        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }

        #region Inputs

        private void LookInput(InputAction.CallbackContext context)
        {
            if(!Application.isFocused || !IsOwner) return;
            var lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0, lookVector.x * 0.1f, 0);
            _playerCamera.transform.Rotate(-lookVector.y * 0.1f, 0, 0);
        }
        
        private void JumpInput(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed)
                InvokeRepeating(nameof(Jump), 0, 0.1f);
            if (context.canceled)
                CancelInvoke(nameof(Jump));
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

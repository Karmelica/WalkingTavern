using System.Collections;
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
    
    public class PlayerMovement : NetworkBehaviour, InputSystem_Actions.IPlayerActions
    {
        [Header("Animator Parameters")]
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jumping = Animator.StringToHash("Jumping");


        [SerializeField] private SkinnedMeshRenderer customization;
        
        private InputSystem_Actions _inputActions;
        private InputSystem_Actions.PlayerActions _playerActions;
        
        [Header("Components")]
        private Camera _playerCamera;
        private Animator _animator;
        private Rigidbody _rb;
        //private Collider _coli;
        
        [Header("Private variables")]
        private Vector2 _inputVector;
        private bool _isGrounded;
        private bool _isSprinting;

        #region Network
        
        public override void OnNetworkSpawn()
        {
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();
            if(IsOwner)
            {
                StartCoroutine(WaitForMainCamera());
                customization.enabled = false;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            
            if(!IsOwner) return;
            
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
            _inputActions.Player.Enable();
        }

        private IEnumerator WaitForMainCamera()
        {
            yield return new WaitUntil(() => Camera.main);
            _playerCamera = Camera.main;
        }

        #endregion

        #region Unity

        // Update is called once per frame
        private void FixedUpdate()
        {
            if(IsOwner) Move();
        }

        private void Update()
        {
            if (!IsOwner) return;
            if (!_playerCamera) return;
            _isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
            Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.2f, Color.red);
            SetAnimationServerRpc(_rb.linearVelocity.magnitude, _isGrounded);
            _playerCamera.transform.position = transform.position + new Vector3(0, 1.7f, 0);
            var lookVectorY = Mathf.Clamp(NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x), -87f,
                87f);
            _playerCamera.transform.localRotation =
                Quaternion.Euler(new Vector3(lookVectorY, transform.localRotation.eulerAngles.y, 0));
        }

        [ServerRpc]
        private void SetAnimationServerRpc(float walkSpeed, bool isGrounded, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            SetAnimationClientRpc(walkSpeed, isGrounded, clientId);
        }
        
        [ClientRpc]
        private void SetAnimationClientRpc(float walkSpeed, bool isGrounded, ulong clientId)
        {
            if(OwnerClientId != clientId) return;
            _animator.SetFloat(WalkSpeed, walkSpeed);
            _animator.SetBool(IsGrounded, isGrounded);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            {
                if(_inputActions == null) return;
            
                _inputActions.Player.Disable();
                _inputActions.Dispose();
            }
        }

        #endregion
        
        private void Jump()
        {
            if (!_isGrounded) return;
            _animator.SetTrigger(Jumping);
            _rb.AddForce(Vector3.up * 5, ForceMode.Impulse);
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

        #region Input Callbacks

        /// <summary>
        /// Obsługuje input patrzenia (ruch myszy/pada)
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            if(!Application.isFocused || !IsOwner || _playerCamera == null) return;
            
            var lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0, lookVector.x * 0.1f, 0);
            _playerCamera.transform.Rotate(-lookVector.y * 0.1f, 0, 0);
        }
        
        /// <summary>
        /// Obsługuje input skoku
        /// </summary>
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!IsOwner) return;
            if (context.performed)
                Jump();
        }
        
        /// <summary>
        /// Obsługuje input ruchu (WASD/analog)
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
        }
        
        /// <summary>
        /// Obsługuje input sprintu
        /// </summary>
        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.performed;
        }

        /// <summary>
        /// Obsługuje input ataku
        /// </summary>
        public void OnAttack(InputAction.CallbackContext context)
        {
            // TODO: Implementacja ataku
        }

        /// <summary>
        /// Obsługuje input kucania
        /// </summary>
        public void OnCrouch(InputAction.CallbackContext context)
        {
            // TODO: Implementacja kucania
        }

        /// <summary>
        /// Obsługuje input interakcji
        /// </summary>
        public void OnInteract(InputAction.CallbackContext context)
        {
            // TODO: Implementacja interakcji
        }

        /// <summary>
        /// Obsługuje input przejścia do następnego elementu
        /// </summary>
        public void OnNext(InputAction.CallbackContext context)
        {
            // TODO: Implementacja next
        }

        /// <summary>
        /// Obsługuje input przejścia do poprzedniego elementu
        /// </summary>
        public void OnPrevious(InputAction.CallbackContext context)
        {
            // TODO: Implementacja previous
        }

        #endregion
    }
}

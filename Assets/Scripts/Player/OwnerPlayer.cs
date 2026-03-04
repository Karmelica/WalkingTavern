using System;
using System.Collections;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Obsługuje ruch gracza, skakanie i input w środowisku sieciowym
    /// </summary>
    [RequireComponent(typeof(NetworkAnimator))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Collider))]
    
    public class OwnerPlayer : NetworkBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region Constants
        
        private const float CameraHeight = 1.6f;
        private const float CameraVerticalClampMin = -87f;
        private const float CameraVerticalClampMax = 87f;
        [SerializeField] private float WalkForce = 20f;
        [SerializeField] private float SprintForce = 25f;
        [SerializeField] private float JumpForce = 250f;
        private const float LookSensitivity = 0.1f;
        private const float InteractRange = 3f;
        
        #endregion
        
        #region Customs

        [Header("Customization")]
        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private SkinnedMeshRenderer[] localPlayerMesh;
        
        #endregion

        #region Components

        [SerializeField] private Transform interactor;

        #endregion
        
        #region Private Fields
        
        private InputSystem_Actions _inputActions;
        private Camera _playerCamera;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private PlayerGUI _playerGUI;
        private Vector2 _inputVector;
        private bool _shouldUpdateInterface = true;
        private bool _isSprinting;
        private bool _currentInteractable;
        private Coroutine _interactionCoroutine;
        private bool _isInteracting;
        private IInteractable _interactObj;
        private bool _canMove = true;
        private bool _isCrouching;
        private Vector3 lastOffsetFromPortal;
        private Vector3 interactorOffset;
        private NetworkedPlayer _networkedPlayer;

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _networkedPlayer =  GetComponent<NetworkedPlayer>();
        }
        
        private void Update()
        {
            if (!_playerCamera) return;
            
            SetAnimationServerRpc(_inputVector.y, _rigidbody.linearVelocity.magnitude, _isInteracting);
            UpdateInteractorPosition();
            UpdateCameraPosition();
        }

        private void FixedUpdate()
        {
            Move();
        }

        #endregion

        #region Network Lifecycle
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if(!IsOwner) enabled = false;
            
            _playerCamera = Camera.main;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            foreach (var playerMesh in localPlayerMesh) playerMesh.enabled = false;
            _networkedPlayer.SetSteamNickname(SteamClient.SteamId.Value);

            _playerGUI = FindFirstObjectByType<PlayerGUI>();
        
            InitializeInput();
            playerNameCanvas.enabled = false;
            
            StartCoroutine(UpdateInterface());
        }
        

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _interactObj?.PrimaryInteract(this, false);
            _shouldUpdateInterface = false;
            CleanupInput();
        }

        private IEnumerator UpdateInterface()
        {
            while (_shouldUpdateInterface)
            {
                if(GetHitInfo(out IInteractable interactable))
                {
                    if (!interactable.IsInteractedWith())
                    {
                        _playerGUI.interactText.text = $"Interact with {interactable.GetInteractName()}";
                    }
                }
                else
                {
                    _playerGUI.interactText.text = string.Empty;
                }
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        #endregion
        
        #region Input Initialization
        
        /// <summary>
        /// Inicjalizuje system inputów dla właściciela
        /// </summary>
        private void InitializeInput()
        {
            if (_inputActions != null) return;
            
            _inputActions = new InputSystem_Actions();
            _inputActions.Player.SetCallbacks(this);
            _inputActions.Player.Enable();
        }
        
        /// <summary>
        /// Czyści system inputów
        /// </summary>
        private void CleanupInput()
        {
            if (_inputActions == null) return;
            
            _inputActions.Player.Disable();
            _inputActions.Dispose();
            _inputActions = null;
        }
        
        #endregion
        
        #region Ground Check & Camera
        
        /// <summary>
        /// Aktualizuje pozycję i rotację kamery gracza
        /// </summary>
        private void UpdateCameraPosition()
        {
            if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out var hitInfo, InteractRange))
            {
                if(hitInfo.collider.TryGetComponent(out PortalCamera portalCamera))
                {
                    interactorOffset = portalCamera.globalOffset;
                }
                else
                {
                    interactorOffset = Vector3.zero;
                }
            }
            
            var cameraHeight = _isCrouching ? CameraHeight / 2f : CameraHeight;
            _playerCamera.transform.position = transform.position + Vector3.up * cameraHeight;
            
            var lookVectorY = Mathf.Clamp(
                NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x),
                CameraVerticalClampMin,
                CameraVerticalClampMax
            );
            
            _playerCamera.transform.rotation = Quaternion.Euler(lookVectorY, transform.rotation.eulerAngles.y, 0f);
            
        }
        
        private void UpdateInteractorPosition()
        {
            interactor.position = _playerCamera.transform.position - interactorOffset;
            interactor.rotation = Quaternion.Euler(_playerCamera.transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        }
        
        /// <summary>
        /// Normalizuje kąt do zakresu -180 do 180
        /// </summary>
        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            return angle;
        }
        
        #endregion
        
        #region Movement & Physics

        private void Move()
        {
            _rigidbody.AddForce(-new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z), ForceMode.VelocityChange);
            
            if (_networkedPlayer.IsGrounded)
            {
                _rigidbody.AddForce(Vector3.down * (0.2f * Time.fixedDeltaTime), ForceMode.Force);
            }
            
            if(_canMove){
                var moveForce = _isSprinting ? SprintForce : WalkForce;
                var moveVector = (_inputVector.y * transform.forward + _inputVector.x * transform.right).normalized *
                                 (moveForce * Time.fixedDeltaTime);

                if (_rigidbody.linearVelocity.magnitude < moveForce)
                {
                    _rigidbody.AddForce(moveVector, ForceMode.VelocityChange);
                }
            }
        }
        
        /// <summary>
        /// Wykonuje skok
        /// </summary>
        private void Jump()
        {
            if (!_networkedPlayer.IsGrounded) return;

            _networkedPlayer.SetJumping();
            _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Force);
        }
        
        #endregion
        
        
        #region Network RPCs
        
        /// <summary>
        /// Wysyła dane animacji do serwera
        /// </summary>
        [Rpc(SendTo.Server)]
        private void SetAnimationServerRpc(float walkDir, float velocity, bool isInteracting)
        {
            _networkedPlayer.SetAnimationRpc(walkDir, velocity, isInteracting);
        }
        
        #endregion
        

        #region Input Callbacks

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!Application.isFocused || _playerCamera == null || interactor == null || !_canMove) return;
            
            var lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0f, lookVector.x * LookSensitivity, 0f);
            _playerCamera.transform.Rotate(-lookVector.y * LookSensitivity, 0f, 0f);
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!context.started || !_canMove) return;
            Jump();
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            _inputVector = context.ReadValue<Vector2>();
        }
        
        public void OnSprint(InputAction.CallbackContext context)
        {
            _isSprinting = context.performed;
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                if (GetHitInfo(out var interactObj))
                {
                    if (interactObj.IsInteractedWith()) return;
                    _interactObj = interactObj;
                    _interactObj.PrimaryInteract(this, true);
                    _isInteracting = true;
                }
            }
            
            if (context.canceled)
            {
                if (_interactObj == null) return;
                _isInteracting = false;
                _interactObj.PrimaryInteract(this, false);
                _interactObj = null;
            }
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                if(GetHitInfo(out var interactable))
                {
                    interactable.SecondaryInteract(this);
                }
            }
        }

        private bool GetHitInfo(out IInteractable interactableComponent)
        {
            interactableComponent = null;
            if (_isInteracting)
            {
                return false;
            }
            var interactPoint = interactor;
            var ray = new Ray(interactPoint.position, interactPoint.forward);
            if (!Physics.Raycast(ray, out var rayHitInfo, InteractRange, ~LayerMask.NameToLayer("Interactable"))) return false;
            if (rayHitInfo.collider.TryGetComponent(out interactableComponent))
            {
                return true;
            }
            return false;
        }

        public Transform GetInteractPoint()
        {
            return interactor;
        }

        public void SetCanMove(bool canMove)
        {
            _canMove = canMove;
            _rigidbody.linearDamping = float.PositiveInfinity;
            _rigidbody.linearDamping = 0f;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !canMove;
        }

        public bool CanMove()
        {
            return _canMove;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        #endregion
    }
}

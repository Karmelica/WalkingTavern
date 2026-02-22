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
    //[RequireComponent(typeof(CharacterController))]
    
    public class Player : NetworkBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region Constants
        
        private const float CameraHeight = 1.6f;
        private const float Height = 1.8f;
        private const float CameraVerticalClampMin = -87f;
        private const float CameraVerticalClampMax = 87f;
        private const float WalkForce = 200f;
        private const float SprintForce = 250f;
        private const float JumpForce = 250f;
        private const float LookSensitivity = 0.1f;
        private const float InteractRange = 3f;
        
        #endregion
        
        #region Animator Parameters
        
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int WalkDir = Animator.StringToHash("WalkDir");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int IsInteracting = Animator.StringToHash("IsInteracting");

        #endregion

        #region Customs

        [Header("Customization")]
        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private TextMeshProUGUI steamNickname;
        [SerializeField] private SkinnedMeshRenderer[] localPlayerMesh;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname");
        
        #endregion

        #region Components

        [SerializeField] private Transform interactor;
        [SerializeField] private CanvasScript playerGUICanvas;

        #endregion
        
        #region Private Fields
        
        private InputSystem_Actions _inputActions;
        private Camera _playerCamera;
        private Animator _animator;
        //private CharacterController _charController;
        private Rigidbody _rigidbody;
        private Collider _collider;
        private Vector2 _inputVector;
        private NetworkVariable<bool> _isGrounded = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private bool _isSprinting;
        private bool _currentInteractable;
        private Coroutine _interactionCoroutine;
        private bool _isInteracting;
        private IInteractable _interactObj;
        private bool _canMove = true;
        private bool _isCrouching;
        private Vector3 lastOffsetFromPortal;
        private Vector3 interactorOffset;

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            //_charController = GetComponent<CharacterController>();
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        private void Update()
        {
            SetAnimationVariables();
            
            if (!IsOwner) return;
            if (!_playerCamera) return;
            //SetAnimationServerRpc(_inputVector.y, _charController.velocity.magnitude, _isInteracting);
            SetAnimationServerRpc(_inputVector.y, _rigidbody.linearVelocity.magnitude, _isInteracting);
            if (!_canMove) return;
            UpdateInteractorPosition();
            UpdateCameraPosition();
        }

        private void FixedUpdate()
        {
            if (IsOwner && _canMove) Move();
        }

        #endregion

        #region Network Lifecycle
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsOwner)
            {
                if(!_playerCamera)
                {
                    _playerCamera = Camera.main;
                }
                
                foreach (var playerMesh in localPlayerMesh) playerMesh.enabled = false;
                SetSteamNicknameServerRpc(SteamClient.SteamId.Value);
                playerGUICanvas.gameObject.SetActive(true);
                InitializeInput();
                playerNameCanvas.enabled = false;
            }

            _playerNickname.OnValueChanged += SetNickname;
            SetNickname("Nickname", _playerNickname.Value);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(UpdateInterface());
        }

        private IEnumerator UpdateInterface()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                UpdatePlayerNickRotation();
                if(!IsOwner || !_playerCamera) continue;
                var interactable = GetHitInfo();
                playerGUICanvas.interactText.text = interactable != null && !interactable.IsInteractedWith()
                    ? $"Interact with {interactable.GetInteractName()}"
                    : string.Empty;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _interactObj?.PrimaryInteract(this, false);
            if (IsOwner && _playerCamera != null)
            {
                Destroy(_playerCamera.gameObject);
            }
            CleanupInput();
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
        
        private void UpdatePlayerNickRotation()
        {
            if(playerNameCanvas && _playerCamera)
                playerNameCanvas.transform.forward = _playerCamera.transform.forward;
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
            _isGrounded.Value = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
            
            var moveForce = _isSprinting ? SprintForce : WalkForce;
            var moveVector = (_inputVector.y * transform.forward + _inputVector.x * transform.right).normalized * (moveForce * Time.fixedDeltaTime);
            
            moveVector.y = _rigidbody.linearVelocity.y;
            
            _rigidbody.linearVelocity = moveVector;
        }
        
        /// <summary>
        /// Wykonuje skok
        /// </summary>
        private void Jump()
        {
            if (!_isGrounded.Value) return;
            
            _animator.SetTrigger(Jumping);
            _rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.Force);
        }
        
        #endregion
        
        private void SetAnimationVariables()
        {
            _animator.SetBool(IsGrounded, _isGrounded.Value);
        }
        
        #region Network RPCs
        
        /// <summary>
        /// Wysyła dane animacji do serwera
        /// </summary>
        [ServerRpc]
        private void SetAnimationServerRpc(float walkDir, float velocity, bool isInteracting, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            SetAnimationClientRpc(walkDir, velocity, isInteracting, clientId);
        }
        
        /// <summary>
        /// Synchronizuje animacje dla wszystkich klientów
        /// </summary>
        [ClientRpc]
        private void SetAnimationClientRpc(float walkDir, float velocity, bool isInteracting, ulong clientId)
        {
            if (OwnerClientId != clientId) return;
            
            _animator.SetBool(IsInteracting, isInteracting);
            _animator.SetFloat(WalkSpeed, velocity);
            _animator.SetFloat(WalkDir, Mathf.Abs(walkDir) > 0 ? walkDir : 1f);
        }

        /// <summary>
        /// Pobiera nick ze Steam i ustawia go dla tego gracza
        /// </summary>
        [ServerRpc(InvokePermission = RpcInvokePermission.Everyone)]
        private void SetSteamNicknameServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
        {
            _playerNickname.Value = new Friend(id).Name;
        }
        
        /// <summary>
        /// Ustawia nick przy wejściu klienta
        /// </summary>
        private void SetNickname(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            steamNickname.text = _playerNickname.Value.ToString();
        }

        #endregion

        #region Input Callbacks

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!Application.isFocused || !IsOwner || _playerCamera == null || interactor == null || !_canMove) return;
            
            var lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0f, lookVector.x * LookSensitivity, 0f);
            _playerCamera.transform.Rotate(-lookVector.y * LookSensitivity, 0f, 0f);
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!IsOwner || !context.started || !_canMove) return;
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
                var interactObj = GetHitInfo();

                if (interactObj == null) return;
                if (interactObj.IsInteractedWith()) return;
                _interactObj = interactObj;
                _interactObj.PrimaryInteract(this, true);
                _isInteracting = true;
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
                var interactable = GetHitInfo();
                if( interactable == null) return;
                interactable.SecondaryInteract(this);
            }
        }

        public void SetCanMove(bool canMove)
        {
            if(!IsOwner) return;
            _canMove = canMove;
            Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !canMove;
        }

        public bool CanMove()
        {
            return _canMove;
        }

        private IInteractable GetHitInfo()
        {
            if (_isInteracting) return null;
            var interactPoint = interactor;
            var ray = new Ray(interactPoint.position, interactPoint.forward);
            var rayHitInfo = Physics.RaycastAll(ray, InteractRange);
            foreach (var hitInfo in rayHitInfo)
            {
                if (hitInfo.collider.TryGetComponent(out IInteractable interactObj))
                {
                    return interactObj;
                }
            }
            return null;
        }

        public Transform GetInteractPoint()
        {
            return interactor;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        }

        #endregion
    }
}

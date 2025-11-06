using System.Collections;
using Steamworks;
using Steamworks.Data;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Color = UnityEngine.Color;

namespace Player
{
    /// <summary>
    /// Obsługuje ruch gracza, skakanie i input w środowisku sieciowym
    /// </summary>
    [RequireComponent(typeof(NetworkAnimator))]
    [RequireComponent(typeof(NetworkTransform))]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkRigidbody))]
    [RequireComponent(typeof(Collider))]
    
    public class Player : NetworkBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region Constants
        
        private const float CameraHeightOffset = 1.7f;
        private const float CameraVerticalClampMin = -87f;
        private const float CameraVerticalClampMax = 87f;
        private const float GroundCheckDistance = 0.2f;
        private const float GroundCheckOffset = 0.1f;
        private const float WalkForce = 10f;
        private const float SprintForce = 20f;
        private const float MaxWalkSpeed = 2.5f;
        private const float MaxSprintSpeed = 5f;
        private const float JumpForce = 5f;
        private const float LookSensitivity = 0.1f;
        private const float InteractRange = 3f;
        
        #endregion
        
        #region Animator Parameters
        
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int WalkDir = Animator.StringToHash("WalkDir");
        private static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        
        #endregion

        #region Customs

        [Header("Customization")]
        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private TextMeshProUGUI steamNickname;
        [SerializeField] private SkinnedMeshRenderer localPlayerMesh;
        private NetworkVariable<FixedString64Bytes> playerNickname = new("Nickname");
        
        #endregion

        #region Components

        [SerializeField] private GameObject playerCameraPrefab;

        #endregion
        
        #region Private Fields
        
        private InputSystem_Actions _inputActions;
        private Camera _playerCamera;
        private Animator _animator;
        private Rigidbody _rb;
        private Vector2 _inputVector;
        private bool _isGrounded;
        private bool _isSprinting;
        private bool _currentInteractable;
        private Coroutine _interactionCoroutine;
        private bool _isInteracting;
        private IInteractable _interactObj;

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            // Cache component references
            _animator = GetComponent<Animator>();
            _rb = GetComponent<Rigidbody>();
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            if (_playerCamera == null) return;
            
            UpdateGroundCheck();
            UpdateCameraPosition();
            SetAnimationServerRpc(_rb.linearVelocity.magnitude, _inputVector.y, _isGrounded);
            
            if (_isInteracting && _interactObj != null)
            {
                PerformInteraction(_interactObj, _playerCamera.transform);
            }
        }
        
        private void FixedUpdate()
        {
            if (IsOwner) 
                Move();
        }
        
        #endregion

        #region Network Lifecycle
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsOwner)
            {
                Debug.Log("Local player spawned.");
                if(_playerCamera == null)
                {
                    _playerCamera = Instantiate(playerCameraPrefab).GetComponent<Camera>();
                    DontDestroyOnLoad(_playerCamera.gameObject);
                }
                localPlayerMesh.enabled = false;
                InitializeInput();
                SetSteamNicknameServerRpc(SteamClient.SteamId.Value);
            }

            playerNickname.OnValueChanged += SetNickname;
            SetNickname("Nickname", playerNickname.Value);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public override void OnNetworkPreDespawn()
        {
            base.OnNetworkPreDespawn();
            if (IsOwner && _playerCamera != null)
            {
                Destroy(_playerCamera.gameObject);
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
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
        /// Sprawdza czy gracz dotyka ziemi
        /// </summary>
        private void UpdateGroundCheck()
        {
            var rayOrigin = transform.position + Vector3.up * GroundCheckOffset;
            _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, GroundCheckDistance);
            Debug.DrawRay(rayOrigin, Vector3.down * GroundCheckDistance, Color.red);
        }
        
        /// <summary>
        /// Aktualizuje pozycję i rotację kamery gracza
        /// </summary>
        private void UpdateCameraPosition()
        {
            _playerCamera.transform.position = transform.position + Vector3.up * CameraHeightOffset;
            
            var lookVectorY = Mathf.Clamp(
                NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x),
                CameraVerticalClampMin,
                CameraVerticalClampMax
            );
            
            _playerCamera.transform.localRotation = Quaternion.Euler(
                lookVectorY,
                transform.localRotation.eulerAngles.y,
                0f
            );
        }
        
        /// <summary>
        /// Normalizuje kąt do zakresu -180 do 180
        /// </summary>
        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f) 
                angle -= 360f;
            return angle;
        }
        
        #endregion
        
        #region Movement & Physics
        
        /// <summary>
        /// Obsługuje ruch gracza używając fizyki
        /// </summary>
        private void Move()
        {
            var moveVector = _inputVector.y * transform.forward + _inputVector.x * transform.right;
            var moveForce = _isSprinting ? SprintForce : WalkForce;
            
            _rb.AddForce(moveVector * moveForce, ForceMode.Force);
            
            // Limit horizontal velocity
            var maxSpeed = _isSprinting ? MaxSprintSpeed : MaxWalkSpeed;
            if (!(_rb.linearVelocity.magnitude > maxSpeed)) return;
            var limitedVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
            limitedVelocity = limitedVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.y);
        }
        
        /// <summary>
        /// Wykonuje skok jeśli gracz dotyka ziemi
        /// </summary>
        private void Jump()
        {
            if (!_isGrounded) return;
            
            _animator.SetTrigger(Jumping);
            _rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
        }
        
        #endregion
        
        #region Network RPCs
        
        /// <summary>
        /// Wysyła dane animacji do serwera
        /// </summary>
        [ServerRpc]
        private void SetAnimationServerRpc(float walkSpeed, float walkDir, bool isGrounded, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId;
            SetAnimationClientRpc(walkSpeed, walkDir, isGrounded, clientId);
        }
        
        /// <summary>
        /// Synchronizuje animacje dla wszystkich klientów
        /// </summary>
        [ClientRpc]
        private void SetAnimationClientRpc(float walkSpeed, float walkDir, bool isGrounded, ulong clientId)
        {
            if (OwnerClientId != clientId) return;
            
            _animator.SetFloat(WalkSpeed, walkSpeed);
            _animator.SetFloat(WalkDir, Mathf.Abs(walkDir) > 0 ? walkDir : 1f);
            _animator.SetBool(IsGrounded, isGrounded);
        }

        /// <summary>
        /// Pobiera nick ze Steam i ustawia go dla tego gracza
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void SetSteamNicknameServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
        {
            //var clientId = serverRpcParams.Receive.SenderClientId;
            playerNickname.Value = new Friend(id).Name;
            //SetSteamNicknameClientRpc(clientId);
        }
        
        /// <summary>
        /// Ustawia nick przy wejściu klienta
        /// </summary>
        private void SetNickname(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            steamNickname.text = playerNickname.Value.ToString();
        }

        #endregion

        #region Input Callbacks

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!Application.isFocused || !IsOwner || _playerCamera == null) return;
            
            var lookVector = context.ReadValue<Vector2>();
            transform.Rotate(0f, lookVector.x * LookSensitivity, 0f);
            _playerCamera.transform.Rotate(-lookVector.y * LookSensitivity, 0f, 0f);
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (!IsOwner || !context.started) return;
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
                var interactPoint = _playerCamera.transform;
                var ray = new Ray(interactPoint.position, interactPoint.forward);
                if (!Physics.Raycast(ray, out var hitInfo, InteractRange)) return;
                if (!hitInfo.collider.TryGetComponent(out IInteractable interactObj)) return;
                _isInteracting = true;
                _interactObj = interactObj;
            }
            
            if (context.canceled)
            {
                if (_interactObj == null) return;
                _isInteracting = false;
                _interactObj.PrimaryInteract();
                _interactObj = null;
            }
        }

        private void PerformInteraction(IInteractable interactObj, Transform interactPoint)
        {
            interactObj.PrimaryInteract(interactPoint);
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            // TODO: Implementacja kucania
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            // TODO: Implementacja interakcji / skillchecków
        }

        #endregion
    }
}


public interface IInteractable
{
    public void PrimaryInteract(Transform interactor = null);
}

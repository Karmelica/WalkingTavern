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
    [RequireComponent(typeof(CharacterController))]
    
    public class Player : NetworkBehaviour, InputSystem_Actions.IPlayerActions
    {
        #region Constants
        
        private const float CameraHeight = 1.7f;
        private const float Height = 1.8f;
        private const float CameraVerticalClampMin = -87f;
        private const float CameraVerticalClampMax = 87f;
        private const float GroundCheckDistance = 0.2f;
        private const float GroundCheckOffset = 0.1f;
        private const float WalkForce = 5f;
        private const float SprintForce = 7.5f;
        //private const float MaxWalkSpeed = 2.5f;
        //private const float MaxSprintSpeed = 5f;
        private const float JumpForce = 1f;
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
        [SerializeField] private SkinnedMeshRenderer localPlayerMesh;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname");
        
        #endregion

        #region Components

        [SerializeField] private GameObject playerCameraPrefab;
        [SerializeField] private Transform interactor;
        [SerializeField] private CanvasScript canvasScript;

        #endregion
        
        #region Private Fields
        
        private InputSystem_Actions _inputActions;
        private Camera _playerCamera;
        private Animator _animator;
        private CharacterController _charController;
        private Vector2 _inputVector;
        private Vector3 _playerVelocity;
        private NetworkVariable<bool> _isGrounded = new(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private bool _isSprinting;
        private bool _currentInteractable;
        private Coroutine _interactionCoroutine;
        private bool _isInteracting;
        private IInteractable _interactObj;
        private bool _canMove = true;
        private bool _isCrouching;

        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _charController = GetComponent<CharacterController>();
        }
        
        private void Update()
        {
            SetAnimationVariables();
            UpdateInteractorPosition();
            
            if (!IsOwner) return;
            if (!_playerCamera) return;
            SetAnimationServerRpc(_inputVector.y, _charController.velocity.magnitude, _isInteracting);
            if (!_canMove) return;
            UpdateCameraPosition();
        }

        private void FixedUpdate()
        {
            if (IsOwner && _canMove)
                Move();
        }
        
        #endregion

        #region Network Lifecycle
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsOwner)
            {
                //Debug.Log("Local player spawned.");
                if(!_playerCamera)
                {
                    _playerCamera = Instantiate(playerCameraPrefab).GetComponent<Camera>();
                }
                
                localPlayerMesh.enabled = false;
                SetSteamNicknameServerRpc(SteamClient.SteamId.Value);
                canvasScript.gameObject.SetActive(true);
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
                canvasScript.interactText.text = interactable != null && !interactable.IsInteractedWith()
                    ? $"Interact with {interactable.GetInteractName()}"
                    : string.Empty;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
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
            _playerCamera.transform.position = interactor.position;
            _playerCamera.transform.rotation = Quaternion.Euler(interactor.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0f);
        }
        
        private void UpdatePlayerNickRotation()
        {
            if(playerNameCanvas && _playerCamera)
                playerNameCanvas.transform.forward = _playerCamera.transform.forward;
        }
        
        private void UpdateInteractorPosition()
        {
            var cameraHeight = _isCrouching ? CameraHeight / 2f : CameraHeight;
            interactor.position = transform.position + Vector3.up * cameraHeight;
            
            var lookVectorY = Mathf.Clamp(
                NormalizeAngle(interactor.rotation.eulerAngles.x),
                CameraVerticalClampMin,
                CameraVerticalClampMax
            );
            
            interactor.localRotation = Quaternion.Euler(lookVectorY, 0f, 0f);
        }
        
        /// <summary>
        /// Normalizuje kąt do zakresu -180 do 180
        /// </summary>
        private static float NormalizeAngle(float angle)
        {
            if (angle > 180f) angle -= 360f;
            //if( angle < -180f) angle += 360f;
            return angle;
        }
        
        #endregion
        
        #region Movement & Physics

        private void Move()
        {
            _isGrounded.Value = _charController.isGrounded;

            if (_isGrounded.Value)
            {
                if(_playerVelocity.y < -2f)
                    _playerVelocity.y = -2f;
            }
            
            var moveVector = (_inputVector.y * transform.forward + _inputVector.x * transform.right).normalized;
            var moveForce = _isSprinting ? SprintForce : WalkForce;
            
            _playerVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
            
            _charController.Move((moveVector * moveForce + Vector3.up * _playerVelocity.y) * Time.fixedDeltaTime);
            
            /*_rb.AddForce(moveVector * moveForce, ForceMode.Force);

            // Limit horizontal velocity
            var maxSpeed = _isSprinting ? MaxSprintSpeed : MaxWalkSpeed;
            if (!(_rb.linearVelocity.magnitude > maxSpeed)) return;
            var limitedVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z);
            limitedVelocity = limitedVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(limitedVelocity.x, _rb.linearVelocity.y, limitedVelocity.y);*/
        }
        
        /// <summary>
        /// Wykonuje skok jeśli gracz dotyka ziemi
        /// </summary>
        private void Jump()
        {
            if (!_isGrounded.Value) return;
            
            _animator.SetTrigger(Jumping);
            _playerVelocity.y = Mathf.Sqrt(JumpForce * -2f * Physics.gravity.y);
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
            //var clientId = serverRpcParams.Receive.SenderClientId;
            _playerNickname.Value = new Friend(id).Name;
            //SetSteamNicknameClientRpc(clientId);
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
            interactor.Rotate(-lookVector.y * LookSensitivity, 0f, 0f);
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
                _interactObj.PrimaryInteract(this);
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

        private IInteractable GetHitInfo()
        {
            var interactPoint = interactor;
            var ray = new Ray(interactPoint.position, interactPoint.forward);
            if (!Physics.Raycast(ray, out var hitInfo, InteractRange)) return null;
            return hitInfo.collider.TryGetComponent(out IInteractable interactObj) ? interactObj : null;
        }

        public Transform GetInteractPoint()
        {
            return interactor;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if(!IsOwner) return;
            if (context.started)
            {
                _isCrouching = true;
                var newHeight = _charController.height = Height / 2f;
                _charController.center = new Vector3(0f, newHeight / 2f, 0f);
            }
            if (context.canceled)
            {
                _isCrouching = false;
                var newHeight = _charController.height = Height;
                _charController.center = new Vector3(0f, newHeight / 2f, 0f);
            }
        }

        #endregion
    }
}

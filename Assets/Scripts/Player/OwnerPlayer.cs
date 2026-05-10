using System;
using System.Collections;
using JetBrains.Annotations;
using MyInterfaces;
using Steamworks;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Rendering.Universal;
using World.Caravan;

namespace PlayerScripts
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
        #region Variables

        [SerializeField] private ScriptableRendererFeature renderOutlines;
        
        #region Constants
        
        private const float CameraHeight = 1.6f;
        private const float CameraVerticalClampMin = -87f;
        private const float CameraVerticalClampMax = 87f;
        [SerializeField] private float walkForce = 20f;
        [SerializeField] private float sprintForce = 25f;
        [SerializeField] private float jumpForce = 250f;
        [Range(0f, 90f)]
        [SerializeField] private float walkableAngle = 45f;
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
        [SerializeField] private Transform hand;

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
        private bool _isInteracting;
        [CanBeNull] private IInteractable _lastInteractedObject;
        private bool _canMove = true;
        private bool _isCooking;
        private bool _isDriving;
        private bool _isHoldingMouseButton;
        private Vector3 _lastOffsetFromPortal;
        private Transform _minigameCamera;
        private NetworkedPlayer _networkedPlayer;
        private CaravanControlScript _caravanControl;
        private Matrix4x4 _windowMatrix;

        #endregion
        
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
            UpdateInteractorPosition();
            UpdateCameraPosition();
            if(!_isDriving){
                SetAnimationServerRpc(_inputVector.y, _rigidbody.linearVelocity.magnitude, _lastInteractedObject != null);
            }
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

            if (!IsOwner) {
                enabled = false;
                _shouldUpdateInterface = false;
            }
            else {
                _playerCamera = Camera.main;
                _windowMatrix = _playerCamera!.transform.localToWorldMatrix;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                foreach (var playerMesh in localPlayerMesh) playerMesh.enabled = false;
                _networkedPlayer.SetSteamNicknameRpc(SteamClient.SteamId.Value);
                _networkedPlayer.SetSkinRpc(PlayerPrefs.GetInt("PlayerSkin", 0));
                _networkedPlayer.SetFaceRpc(PlayerPrefs.GetInt("PlayerFace", 0));
                _networkedPlayer.SetEarsRpc(PlayerPrefs.GetInt("PlayerEars", 0));
                _networkedPlayer.SetPantsRpc(PlayerPrefs.GetInt("PlayerPants", 0));
                _networkedPlayer.SetShirtRpc(PlayerPrefs.GetInt("PlayerShirt", 0));
                _networkedPlayer.SetHairRpc(PlayerPrefs.GetInt("PlayerHair", 0));
                _networkedPlayer.SetPantsColorRpc(PlayerPrefs.GetInt("PlayerPantsColor", 0));
                _networkedPlayer.SetShirtColorRpc(PlayerPrefs.GetInt("PlayerShirtColor", 0));
                _networkedPlayer.SetHairColorRpc(PlayerPrefs.GetInt("PlayerHairColor", 0));

                _playerGUI = FindFirstObjectByType<PlayerGUI>();

                InitializeInput();
                playerNameCanvas.enabled = false;

                StartCoroutine(UpdateInterface());
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _lastInteractedObject?.PickupOrDropObject(false, transform.position);
            _lastInteractedObject = null;
            _shouldUpdateInterface = false;
            CleanupInput();
        }

        private IEnumerator UpdateInterface()
        {
            while (_shouldUpdateInterface) {
                if(GetHitInfo(out var interactable, out _, true, QueryTriggerInteraction.Collide)) {
                    if (!interactable.IsInteractedWith() && !_isInteracting && !_isCooking) {
                        _playerGUI.interactText.text = interactable.GetInteractText();
                    }
                    else {
                        _playerGUI.interactText.text = string.Empty;
                    }
                }
                else {
                    _playerGUI.interactText.text = string.Empty;
                }
                yield return new WaitForSeconds(0.1f);
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
            if (!_isCooking) {
                _windowMatrix = _playerCamera.transform.localToWorldMatrix;
                if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out var hitInfo,
                        InteractRange)) {
                    if (hitInfo.collider.TryGetComponent(out PortalCamera portal)) {
                        _windowMatrix = portal.otherPortal.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * 
                                        _playerCamera.transform.localToWorldMatrix;
                    }
                }
                
                _playerCamera.transform.position = transform.position + Vector3.up * CameraHeight;

                var lookVectorY = Mathf.Clamp(
                    NormalizeAngle(_playerCamera.transform.rotation.eulerAngles.x),
                    CameraVerticalClampMin,
                    CameraVerticalClampMax
                );

                _playerCamera.transform.rotation = Quaternion.Euler(lookVectorY, transform.rotation.eulerAngles.y, 0f);
            }
            else {
                _playerCamera.transform.position = Vector3.Lerp(_playerCamera.transform.position, _minigameCamera.position, 0.5f);
                _playerCamera.transform.rotation = Quaternion.Lerp(_playerCamera.transform.rotation, _minigameCamera.rotation, 0.5f);
                
            }
        }
        
        private void UpdateInteractorPosition()
        {
            interactor.transform.SetPositionAndRotation(_windowMatrix.GetColumn(3), _windowMatrix.rotation);
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

            if(MathF.Abs(transform.eulerAngles.x) > 0.01f || Mathf.Abs(transform.eulerAngles.z) > 0.01f) {
                var rot = Quaternion.identity;
                transform.rotation = new Quaternion(rot.x, transform.rotation.eulerAngles.y, rot.z, rot.w);
            }

            if (_canMove) {
                var moveForce = _isSprinting ? sprintForce : walkForce;
                
                var moveVector = (_inputVector.y * transform.forward + _inputVector.x * transform.right).normalized *
                                 (moveForce * Time.fixedDeltaTime);

                if (_rigidbody.linearVelocity.magnitude < moveForce) {
                    if (Physics.Raycast(transform.position + Vector3.up * 0.05f, moveVector + Vector3.up * -1, out var hit, 0.4f)) {
                        if (Vector3.Angle(hit.normal, Vector3.up) > walkableAngle && !_networkedPlayer.IsGrounded) return;
                    }
                    _rigidbody.AddForce(moveVector, ForceMode.VelocityChange);
                }
            }

            if (_caravanControl) {
                _caravanControl.Drive(_inputVector);
            }
        }

        /// <summary>
        /// Wykonuje skok
        /// </summary>
        private void Jump()
        {
            if (!_networkedPlayer.IsGrounded) return;

            _networkedPlayer.SetJumping();
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Force);
        }
        
        #endregion
        
        #region Network RPCs
        
        /// <summary>
        /// Wysyła dane animacji do serwera
        /// </summary>
        [Rpc(SendTo.Server)]
        private void SetAnimationServerRpc(float walkDir, float velocity, bool isInteracting)
        {
            _networkedPlayer.SetAnimationRpc(walkDir, Mathf.RoundToInt(velocity), isInteracting);
        }
        
        #endregion

        #region Input Callbacks

        public void OnLook(InputAction.CallbackContext context)
        {
            if (!Application.isFocused || _playerCamera == null) return;
            if (!_canMove && !_isDriving) return;
            
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
            if (!context.performed) return;
            
            if (context.interaction is HoldInteraction) {
                _playerGUI.ShowPause(true);
                SetCanMove(false);
            }
            else {
                if (_playerGUI.IsPaused()) {
                    _playerGUI.ShowPause(false);
                    if (!_isCooking) SetCanMove(true);
                    return;
                }
                
                if (_lastInteractedObject != null) {
                    _isInteracting = false;
                    _networkedPlayer.ChangeObjectInHandIdRpc(0);
                    _lastInteractedObject.PickupOrDropObject(false, CalculateDropPoint());
                    _lastInteractedObject = null;
                }

                SetCooking(false);
                SetCanMove(true);
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.started) { _isHoldingMouseButton = true; }
            if (context.canceled) { _isHoldingMouseButton = false; }
            
            if (!_canMove || _isCooking) return;

            if (context.started) {
                if (!GetHitInfo(out var interactObj, out var idComponent)) return;
                if (idComponent != null)
                    _networkedPlayer.ChangeObjectInHandIdRpc(idComponent.ID);
                _lastInteractedObject = interactObj.PickupOrDropObject(true);
                _isInteracting = _lastInteractedObject != null;
            }
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!_canMove || _isCooking) return;

            if (!context.started) return;
            if (GetHitInfo(out var interactable, out _, false) && 
                (interactable.GetInteractText().Contains("Door") || interactable.GetInteractText().Contains("Storage"))) 
            {
                interactable.SecondaryInteract();
                return;
            }
            
            if (_lastInteractedObject != null) {
                _isInteracting = false;
                _networkedPlayer.ChangeObjectInHandIdRpc(0);
                _lastInteractedObject.PickupOrDropObject(false, CalculateDropPoint());
                _lastInteractedObject = null;
                return;
            }
            
            if(GetHitInfo(out interactable, out _,false, QueryTriggerInteraction.Collide)) {
                if (interactable.IsInteractedWith()) return;
                _lastInteractedObject = interactable.SecondaryInteract(this);
            }
        }
        
        public void OnCrouch(InputAction.CallbackContext context)
        {
            if(context.started)
                renderOutlines?.SetActive(true);
            if(context.canceled)
                renderOutlines?.SetActive(false);
        }

        private bool GetHitInfo(out IInteractable interactableComponent, out IObjectID objectID, bool shouldCheckForInteracting = true,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            interactableComponent = null;
            objectID = null;
            
            if (shouldCheckForInteracting && _isInteracting) return false;
            var interactPoint = interactor;
            var ray = new Ray(interactPoint.position, interactPoint.forward);
            var rayHitInfo = Physics.RaycastAll(ray, InteractRange, 1<<7, triggerInteraction);
            Array.Sort(rayHitInfo, Utilis.CompareRaycastDistance);
            foreach (var hit in rayHitInfo) {
                if (hit.collider.TryGetComponent(out interactableComponent)) {
                    hit.collider.TryGetComponent(out objectID);
                    return true;
                }
            }
            return false;
        }

        private Vector3 CalculateDropPoint()
        {
            var hitObjects = Physics.RaycastAll(interactor.position, interactor.forward, 3f, ~(1 << 11), QueryTriggerInteraction.Ignore);
            if (hitObjects.Length > 0) {
                Array.Sort(hitObjects, Utilis.CompareRaycastDistance);
                foreach (var hit in hitObjects) {
                    if (hit.collider.gameObject == _networkedPlayer.ObjectInHand?.gameObject) continue;
                    return hit.point + hit.normal * 0.2f;
                }
            }

            //drop on ground if nothing or only held object was hit
            Physics.Raycast(hand.position, Vector3.down, out var groundHit, Single.PositiveInfinity, ~(1 << 2),
                QueryTriggerInteraction.Ignore);
            return groundHit.point + Vector3.up * 0.2f;
        }
        
        public bool IsHoldingLmb()
        {
            return _isHoldingMouseButton;
        }

        public void SetCameraLocation(Transform cameraLocation)
        {
            _minigameCamera = cameraLocation;
        }
        
        public void SetCanMove(bool canMove, bool changeCursorState = true)
        {
            _canMove = canMove;
            _rigidbody.linearDamping = float.PositiveInfinity;
            _rigidbody.linearDamping = 0f;
            if(changeCursorState){
                Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !canMove;
            }
        }
        
        public void SetCooking(bool cooking)
        {
            _isCooking = cooking;
        }
        
        public void SetDriving(bool driving)
        {
            _isDriving = driving;
            _networkedPlayer.SetDriving(driving);
            SetCanMove(!driving, false);
        }

        public void SetCaravanControl(CaravanControlScript caravanControl)
        {
            _caravanControl = caravanControl;
        }

        #endregion
    }
}

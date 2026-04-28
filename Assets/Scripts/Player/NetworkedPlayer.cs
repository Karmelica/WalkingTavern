using System;
using Cooking;
using Managers;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using World;

namespace PlayerScripts
{
    public class NetworkedPlayer : NetworkBehaviour
    { 
        #region Animator Parameters
        
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int WalkDir = Animator.StringToHash("WalkDir");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int IsInteracting = Animator.StringToHash("IsInteracting");
        private static readonly int Grounded = Animator.StringToHash("IsGrounded");
        private static readonly int IsSitting = Animator.StringToHash("IsSitting");
        private static readonly int StoppedSitting = Animator.StringToHash("StoppedSitting");

        #endregion

        #region Variables

        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private SkinnedMeshRenderer[] networkedSkinMesh;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerEars;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerPants;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerShirt;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerHair;
        [SerializeField] private SkinnedMeshRenderer networkedPlayerFace;
        [SerializeField] private Material[] skins;
        [SerializeField] private Material[] faces;
        private TextMeshProUGUI _steamNicknameTMP;
        
        private readonly NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerSkinIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerFaceIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerEarsIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerPantsIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerShirtIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerHairIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<uint> _objectId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public bool IsGrounded { get; private set; }
        
        private Camera _playerCamera;
        private Animator _animator;

        private int _newVelocity;
        
        [SerializeField] private Transform networkedHandTransform;
        [SerializeField] private Transform localHandTransform;
        private Transform _parentTransform;
        public MoveableObject ObjectInHand { get; private set; }

        #endregion
        
        #region Unity Lifecycle

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            _playerCamera = Camera.main;
            _steamNicknameTMP = playerNameCanvas.GetComponentInChildren<TextMeshProUGUI>();
            
            _playerNickname.OnValueChanged += OnNicknameChanged;
            _playerSkinIndex.OnValueChanged += OnSkinChanged;
            _playerFaceIndex.OnValueChanged += OnFaceChanged;
            _playerEarsIndex.OnValueChanged += OnEarsChanged;
            _playerHairIndex.OnValueChanged += OnHairChanged;
            _playerPantsIndex.OnValueChanged += OnPantsChanged;
            _playerShirtIndex.OnValueChanged += OnShirtChanged;
            
            NicknameChanged(_playerNickname.Value);
            SkinChanged(_playerSkinIndex.Value);
            FaceChanged(_playerFaceIndex.Value);
            EarsChanged(_playerEarsIndex.Value);
            PantsChanged(_playerPantsIndex.Value);
            ShirtChanged(_playerShirtIndex.Value);
            HairChanged(_playerHairIndex.Value);

            _parentTransform = IsOwner ? localHandTransform : networkedHandTransform;
            _objectId.OnValueChanged += ChangeObjectInHand;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _playerNickname.OnValueChanged -= OnNicknameChanged;
            _playerSkinIndex.OnValueChanged -= OnSkinChanged;
            _playerFaceIndex.OnValueChanged -= OnFaceChanged;
            _playerEarsIndex.OnValueChanged -= OnEarsChanged;
            _playerHairIndex.OnValueChanged -= OnHairChanged;
            _playerPantsIndex.OnValueChanged -= OnPantsChanged;
            _playerShirtIndex.OnValueChanged -= OnShirtChanged;
            
            _objectId.OnValueChanged -= ChangeObjectInHand;
        }
        
        private void Update()
        {
            UpdatePlayerNickRotation();
            SetAnimationVariables();
        }

        private void FixedUpdate()
        {
            GroundCheck();
        }

        #endregion
        
        #region Animation and Calculations

        private void UpdatePlayerNickRotation()
        {
            if(playerNameCanvas && _playerCamera)
                playerNameCanvas.transform.forward = _playerCamera.transform.forward;
        }
        
        private void GroundCheck()
        {
            IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
        }
        
        public void SetJumping()
        {
            _animator.SetTrigger(Jumping);
        }
        
        public void SetDriving(bool isDriving)
        {
            _animator.SetTrigger(isDriving ? IsSitting : StoppedSitting);
        }
        
        private void SetAnimationVariables()
        {
            _animator.SetBool(Grounded, IsGrounded);
        }

        public void PlayFootstep()
        {
            AudioManager.Instance.PlayOneShot(AudioEvents.Instance.footsteps, transform.position);
        }
        
        public void PlayJumpSound()
        {
            AudioManager.Instance.PlayOneShot(AudioEvents.Instance.jump, transform.position);
        }

        #endregion

        #region Set Customization

        /// <summary>
        /// Ustawia nick przy wejściu klienta
        /// </summary>
        private void OnNicknameChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            NicknameChanged(newValue);
        }

        private void NicknameChanged(FixedString64Bytes newValue)
        {
            _steamNicknameTMP.text = newValue.ToString();
        }

        private void OnSkinChanged(int previousValue, int newValue)
        {
            SkinChanged(newValue);
        }

        private void SkinChanged(int index)
        {
            foreach (var mesh in networkedSkinMesh)
            {
                mesh.materials = new[] { skins[index] };
            }
        }
        
        private void OnEarsChanged(int previousValue, int newValue)
        {
            EarsChanged(newValue);
        }
        
        private void EarsChanged(int index)
        {
            if (IsOwner) return;
            Utilis.ShowSelectedMesh(networkedPlayerEars, index);
        }
        
        private void OnFaceChanged(int previousValue, int newValue)
        {
            FaceChanged(newValue);
        }

        private void FaceChanged(int index)
        {
            networkedPlayerFace.materials = new[] { new Material (faces[index]) };
        }
        
        private void OnShirtChanged(int previousValue, int newValue)
        {
            ShirtChanged(newValue);
        }

        private void ShirtChanged(int index)
        {
            if (IsOwner) return;
            Utilis.ShowSelectedMesh(networkedPlayerShirt, index);
        }

        private void OnPantsChanged(int previousValue, int newValue)
        {
            PantsChanged(newValue);
        }

        private void PantsChanged(int index)
        {
            if (IsOwner) return;
            Utilis.ShowSelectedMesh(networkedPlayerPants, index);
        }

        private void OnHairChanged(int previousValue, int newValue)
        {
            HairChanged(newValue);
        }

        private void HairChanged(int index)
        {
            if (IsOwner) return;
            Utilis.ShowSelectedMesh(networkedPlayerHair, index);
        }

        #endregion
        
        #region Network RPCs
        
        /// <summary>
        /// Synchronizuje animacje dla wszystkich klientów
        /// </summary>
        [Rpc(SendTo.Owner)]
        public void SetAnimationRpc(float walkDir, int velocity, bool isInteracting)
        {
            _animator.SetFloat(WalkSpeed, velocity);
            _animator.SetBool(IsInteracting, isInteracting);
            _animator.SetFloat(WalkDir, Mathf.Abs(walkDir) > 0 ? walkDir : 1f);
        }
        
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetSteamNicknameRpc(ulong id)
        {
            _playerNickname.Value = new Friend(id).Name;
        }
        
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetSkinRpc(int skinIndex)
        {
            _playerSkinIndex.Value = skinIndex;
        }
        
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetFaceRpc(int skinIndex)
        {
            _playerFaceIndex.Value = skinIndex;
        }
        
        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void SetEarsRpc(int earsIndex)
        {
            _playerEarsIndex.Value = earsIndex;
        }
        
        public void SetPantsRpc(int pantsIndex)
        {
            _playerPantsIndex.Value = pantsIndex;
        }

        public void SetShirtRpc(int shirtIndex)
        {
            _playerShirtIndex.Value = shirtIndex;
        }

        public void SetHairRpc(int hairIndex)
        {
            _playerHairIndex.Value = hairIndex;
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void ChangeObjectInHandIdRpc(uint id)
        {
            _objectId.Value = id;
        }

        private void ChangeObjectInHand(uint oldValue, uint newValue)
        {
            
            if (newValue == 0) {
                Destroy(ObjectInHand.gameObject);
                ObjectInHand = null;
            }
            else
            {
                ObjectInHand = Instantiate(GetItems.GetObjectByID(newValue), _parentTransform.position, Quaternion.LookRotation(transform.forward), _parentTransform);
                ObjectInHand.GetComponent<Collider>().enabled = false;
                if (ObjectInHand is not DishItem item) return;
                item.GetComponentInChildren<Canvas>().enabled = false;
            }
            
        }

        #endregion
    }
}

using System;
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
        
        [SerializeField] private MoveableObject[] itemsInHand;
        private MoveableObject _itemInHand;

        [SerializeField] private Canvas playerNameCanvas;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerMesh;
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerEars;
        [SerializeField] private SkinnedMeshRenderer networkedPlayerFace;
        [SerializeField] private Material[] skins;
        [SerializeField] private Material[] faces;
        private TextMeshProUGUI _steamNicknameTMP;
        
        private readonly NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerSkinIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerFaceIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<int> _playerEarsIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public bool IsGrounded { get; private set; }
        
        private Camera _playerCamera;
        private Animator _animator;

        private int _newVelocity;

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
            
            NicknameChanged(_playerNickname.Value);
            SkinChanged(_playerSkinIndex.Value);
            FaceChanged(_playerFaceIndex.Value);
            EarsChanged(_playerEarsIndex.Value);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            _playerNickname.OnValueChanged -= OnNicknameChanged;
            _playerSkinIndex.OnValueChanged -= OnSkinChanged;
            _playerFaceIndex.OnValueChanged -= OnFaceChanged;
            _playerEarsIndex.OnValueChanged -= OnEarsChanged;
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

        private void SkinChanged(int newValue)
        {
            foreach (var mesh in networkedPlayerMesh)
            {
                mesh.materials = new[] { skins[newValue] };
            }
        }
        
        private void OnEarsChanged(int previousValue, int newValue)
        {
            EarsChanged(newValue);
        }
        
        private void EarsChanged(int newValue)
        {
            if (IsOwner) return;
            for (var i = 0; i < networkedPlayerEars.Length; i++)
            {
                networkedPlayerEars[i].enabled = newValue == i;
            }
        }
        
        private void OnFaceChanged(int previousValue, int newValue)
        {
            FaceChanged(newValue);
        }

        private void FaceChanged(int newValue)
        {
            networkedPlayerFace.materials = new[] { new Material (faces[newValue]) };
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

        [Rpc(SendTo.Everyone)]
        private void ChangeHandItemRpc(int index)
        {
            _itemInHand.gameObject.SetActive(false);
            var item = itemsInHand[index];
            item.gameObject.SetActive(true);
            _itemInHand = item;
        }

        #endregion
    }
}

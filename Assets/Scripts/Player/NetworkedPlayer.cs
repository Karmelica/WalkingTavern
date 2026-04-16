using System;
using Managers;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

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
        [SerializeField] private SkinnedMeshRenderer[] networkedPlayerMesh;
        [SerializeField] private SkinnedMeshRenderer networkedPlayerFace;
        [SerializeField] private Material[] skins;
        [SerializeField] private Material[] faces;
        private TextMeshProUGUI _steamNicknameTMP;
        
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<int> _playerSkinIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<int> _playerFaceIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
            
            _playerNickname.OnValueChanged += SetNickname;
            _playerSkinIndex.OnValueChanged += SetSkin;
            _playerFaceIndex.OnValueChanged += SetFace;
            
            //SetNickname("Nickname", _playerNickname.Value);
            //SetSkin(0, _playerSkinIndex.Value);
            //SetSkin(0, _playerFaceIndex.Value);
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
        private void SetNickname(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            _steamNicknameTMP.text = _playerNickname.Value.ToString();
        }
        
        private void SetSkin(int previousValue, int newValue)
        {
            foreach (var mesh in networkedPlayerMesh)
            {
                mesh.materials = new[] { skins[newValue] };
            }
        }
        
        private void SetFace(int previousValue, int newValue)
        {
            networkedPlayerFace.materials = new[] { new Material (faces[newValue]) };
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

        #endregion

    }
}

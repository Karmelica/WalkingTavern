using System;
using Steamworks;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class NetworkedPlayer : NetworkBehaviour
    { 
        #region Animator Parameters
        
        private static readonly int WalkSpeed = Animator.StringToHash("WalkSpeed");
        private static readonly int WalkDir = Animator.StringToHash("WalkDir");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int IsInteracting = Animator.StringToHash("IsInteracting");
        private static readonly int Grounded = Animator.StringToHash("IsGrounded");

        #endregion
        
        [SerializeField] private Canvas playerNameCanvas;
        private TextMeshProUGUI _steamNicknameTMP;
        
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private NetworkVariable<FixedString64Bytes> _playerNickname = new("Nickname", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public bool IsGrounded { get; private set; }
        
        private Camera _playerCamera;
        private Animator _animator;


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
            SetNickname("Nickname", _playerNickname.Value);
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

        private void GroundCheck()
        {
            IsGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
        }


        private void UpdatePlayerNickRotation()
        {
            if(playerNameCanvas && _playerCamera)
                playerNameCanvas.transform.forward = _playerCamera.transform.forward;
        }
        
        /// <summary>
        /// Ustawia nick przy wejściu klienta
        /// </summary>
        private void SetNickname(FixedString64Bytes previousValue, FixedString64Bytes newValue)
        {
            _steamNicknameTMP.text = _playerNickname.Value.ToString();
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void SetSteamNicknameRpc(ulong id)
        {
            _playerNickname.Value = new Friend(id).Name;
        }
        
        public void SetJumping()
        {
            _animator.SetTrigger(Jumping);
        }
        
        private void SetAnimationVariables()
        {
            _animator.SetBool(Grounded, IsGrounded);
        }
        #region Network RPCs
        
        /// <summary>
        /// Synchronizuje animacje dla wszystkich klientów
        /// </summary>
        [Rpc(SendTo.Owner)]
        public void SetAnimationRpc(float walkDir, float velocity, bool isInteracting)
        {
            _animator.SetBool(IsInteracting, isInteracting);
            _animator.SetFloat(WalkSpeed, velocity);
            _animator.SetFloat(WalkDir, Mathf.Abs(walkDir) > 0 ? walkDir : 1f);
        }

        #endregion

    }
}

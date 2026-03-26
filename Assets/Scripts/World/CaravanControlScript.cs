using System;
using JetBrains.Annotations;
using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World
{
    public class CaravanControlScript : NetworkBehaviour, IInteractable
    {
        private OwnerPlayer _drivingPlayer;
        private Rigidbody _rb;
        private const float Speed = 50f;
        private NetworkVariable<bool> _isDriven = new NetworkVariable<bool>();
        [SerializeField] private GameObject caravan;
        [SerializeField] private GameObject room;
        [SerializeField] private Transform sitLocation;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            caravan.transform.position = Vector3.Lerp(caravan.transform.position, transform.position, 0.5f);
            caravan.transform.rotation = Quaternion.Lerp(caravan.transform.rotation, transform.rotation, 0.5f);
            
            var roomTargetRot = Quaternion.Euler(-transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
            room.transform.localRotation = Quaternion.Lerp(room.transform.rotation, roomTargetRot, 0.5f);

            if (_drivingPlayer)
            {
                _drivingPlayer.transform.position = sitLocation.position + Vector3.down * 0.5f;
            }
        }

        public void Drive(Vector2 inputVector)
        {
            DriveServerRpc(inputVector);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DriveServerRpc(Vector2 inputVector)
        {
            _rb.AddForce(-new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z), ForceMode.VelocityChange);

            if(_rb.linearVelocity.magnitude < Speed){
                var forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                _rb.AddForce(forward * (inputVector.y * Time.fixedDeltaTime * Speed), ForceMode.VelocityChange);
            }
            
            transform.Rotate(transform.up, inputVector.x * inputVector.y * 25f * Time.fixedDeltaTime);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void DriveCarRpc()
        {
            _isDriven.Value = true;
        }

        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void StopDrivingCarRpc()
        {
            _isDriven.Value = false;
            if(_drivingPlayer){
                _drivingPlayer.SetDriving(false);
                _drivingPlayer.SetCaravanControl(null);
                _drivingPlayer = null;
            }
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            if(!startedInteraction)
            {
                StopDrivingCarRpc();
                var right = sitLocation.right;
                right.y = 0;
                right.Normalize();
                interactor.transform.position = sitLocation.position + right * 3f;
            }
            return null;
        }

        public IInteractable SecondaryInteract(OwnerPlayer interactor)
        {
            _drivingPlayer = interactor;
            _drivingPlayer.SetDriving(true);
            _drivingPlayer.transform.position = sitLocation.position + Vector3.down * 0.5f;
            _drivingPlayer.transform.rotation = sitLocation.rotation;
            _drivingPlayer.SetCaravanControl(this);
            DriveCarRpc();
            return this;
        }

        public string GetInteractName()
        {
            return "Caravan";
        }

        public bool IsInteractedWith()
        {
            return _isDriven.Value;
        }

    }
}

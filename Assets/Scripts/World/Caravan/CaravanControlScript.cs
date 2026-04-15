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
        private const float Speed = 80f;
        private NetworkVariable<bool> _isDriven = new NetworkVariable<bool>();
        [SerializeField] private GameObject caravan;
        [SerializeField] private GameObject room;
        [SerializeField] private Transform sitLocation;
        [SerializeField] private float caravanHeight = 0.6f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 3f))
            {
                transform.position = hit.point + Vector3.up * caravanHeight;
            }
            
            //snail control
            if(caravan)
            {
                var d = Vector3.Distance(caravan.transform.position, sitLocation.position);
                if(d > 4){
                    Vector3 point = default;
                    if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit caravanRay, 3f))
                    {
                         point = caravanRay.point + Vector3.up * caravanHeight;
                    }
                    var vector3 = new Vector3(sitLocation.position.x, point.y, sitLocation.position.z);
                    caravan.transform.position = Vector3.Lerp(caravan.transform.position, vector3, 0.01f);
                }

                var rot = Quaternion.LookRotation(sitLocation.position - caravan.transform.position);
                caravan.transform.rotation = Quaternion.Euler(caravan.transform.eulerAngles.x, rot.eulerAngles.y, caravan.transform.eulerAngles.z);
            }

            //room rotation
            if(room){
                var roomTargetRot = Quaternion.Euler(-caravan.transform.localEulerAngles.x, 0, caravan.transform.localEulerAngles.z);
                room.transform.localRotation = Quaternion.Lerp(room.transform.rotation, roomTargetRot, 0.5f);
            }

            //player position
            if (_drivingPlayer)
            {
                _drivingPlayer.transform.position = sitLocation.position + Vector3.down * 0.5f;
            }
        }

        public void Drive(Vector2 inputVector)
        {
            _rb.AddForce(-new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z), ForceMode.VelocityChange);

            if(_rb.linearVelocity.magnitude < Speed && inputVector.y > 0){
                var forward = transform.forward;
                forward.y = 0;
                forward.Normalize();
                _rb.AddForce(forward * (inputVector.y * Time.fixedDeltaTime * Speed), ForceMode.VelocityChange);
            }
            
            transform.Rotate(transform.up, inputVector.x * inputVector.y * 50f * Time.fixedDeltaTime);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetDriveRpc(RpcParams rpcParams = default)
        {
            _isDriven.Value = true;
            var id = rpcParams.Receive.SenderClientId;
            NetworkObject.ChangeOwnership(id);
        }

        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void StopDrivingCarRpc()
        {
            _isDriven.Value = false;
        }
        
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void SetUprightPositionRpc()
        {
            transform.rotation = Quaternion.identity;
        }

        public IInteractable PrimaryInteract(OwnerPlayer interactor, bool startedInteraction = true)
        {
            if(!startedInteraction)
            {
                StopDrivingCarRpc();
                if(_drivingPlayer && _drivingPlayer == interactor){
                    _drivingPlayer.SetDriving(false);
                    _drivingPlayer.SetCaravanControl(null);
                    _drivingPlayer = null;
                }
                var right = sitLocation.right;
                right.y = 0;
                right.Normalize();
                interactor.transform.position = sitLocation.position + right * 3f;
            }
            else
            {
                if(!_isDriven.Value){
                    SetUprightPositionRpc();
                }
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
            SetDriveRpc();
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

        private void OnDrawGizmos()
        {
            if (sitLocation)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(sitLocation.position, 0.5f);
            }
        }
    }
}

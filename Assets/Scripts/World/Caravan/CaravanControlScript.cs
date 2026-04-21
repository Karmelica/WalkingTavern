using PlayerScripts;
using Unity.Netcode;
using UnityEngine;

namespace World.Caravan
{
    public class CaravanControlScript : NetworkBehaviour, IInteractable
    {
        private OwnerPlayer _drivingPlayer;
        private Rigidbody _rb;
        private const float Speed = 80f;
        private NetworkVariable<bool> _isDriven = new NetworkVariable<bool>();
        [SerializeField] private GameObject caravan;
        [SerializeField] private GameObject room;
        [SerializeField] private Transform followLocation;
        [SerializeField] private Transform sitLocation;
        [SerializeField] private Transform snail;
        [SerializeField] private float caravanHeight = 0.3f;
        [SerializeField] private float distance = 3f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if(Physics.Raycast(snail.position, Vector3.down, out RaycastHit hit, 5f))
            {
                var p = hit.point + Vector3.up * 1.2f;
                transform.position = new Vector3(transform.position.x, p.y, transform.position.z);
            }
            
            //caravan control
            if(caravan)
            {
                var d = Vector3.Distance(caravan.transform.position, followLocation.position);
                if(d > distance){
                    Vector3 point = default;
                    if (Physics.Raycast(caravan.transform.position, Vector3.down, out RaycastHit caravanRay, float.PositiveInfinity))
                    {
                         point = caravanRay.point + Vector3.up * caravanHeight;
                    }
                    var vector3 = new Vector3(followLocation.position.x, point.y, followLocation.position.z);
                    caravan.transform.position = Vector3.Lerp(caravan.transform.position, vector3, 0.01f);
                }

                var rot = Quaternion.LookRotation(followLocation.position - caravan.transform.position);
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
                _drivingPlayer.transform.position = sitLocation.position;
            }
        }

        //driving
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
                var right = followLocation.right;
                right.y = 0;
                right.Normalize();
                interactor.transform.position = followLocation.position + right * 3f;
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
            _drivingPlayer.transform.position = followLocation.position + Vector3.down * 0.5f;
            _drivingPlayer.transform.rotation = followLocation.rotation;
            _drivingPlayer.SetCaravanControl(this);
            SetDriveRpc();
            return this;
        }

        public string GetInteractText()
        {
            return "Control Caravan";
        }

        public bool IsInteractedWith()
        {
            return _isDriven.Value;
        }

        private void OnDrawGizmos()
        {
            if (followLocation)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(followLocation.position, 0.5f);
            }
        }
    }
}

using MyInterfaces;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace World.Caravan
{
	public class CaravanControlScript : NetworkBehaviour, IInteractable
	{
		private const float Speed = 6f;
		private static readonly int Moving = Animator.StringToHash("Moving");
		[SerializeField] private GameObject caravan;
		[SerializeField] private GameObject room;
		[SerializeField] private Transform followLocation;
		[SerializeField] private Transform sitLocation;
		[SerializeField] private Transform snail;
		[SerializeField] private float caravanHeight = 0.3f;
		[SerializeField] private float distance = 3f;
		[SerializeField] private Animator animator;
		private readonly NetworkVariable<bool> _isDriven = new();
		private OwnerPlayer _drivingPlayer;
		private Rigidbody _rb;

		private void Awake()
		{
			_rb = GetComponent<Rigidbody>();
		}

		private void Update()
		{
			animator.SetBool(Moving, _rb.linearVelocity.magnitude > 0.1f);

			if (Physics.Raycast(snail.position, Vector3.down, out var hit, 5f)) {
				var point = hit.point + Vector3.up * 1.2f;
				transform.position = new Vector3(transform.position.x, point.y, transform.position.z);
			}

			//caravan control
			if (caravan) {
				var d = Vector3.Distance(caravan.transform.position, followLocation.position);
				if (d > distance) {
					Vector3 height = default;
					if (Physics.Raycast(caravan.transform.position, Vector3.down, out var caravanRay,
						    float.PositiveInfinity)) {
						height = caravanRay.point + Vector3.up * caravanHeight;
					}

					var targetPos = new Vector3(followLocation.position.x, height.y, followLocation.position.z);
					caravan.transform.position =
						Vector3.Lerp(caravan.transform.position, targetPos, 0.5f * Time.deltaTime);
				}

				var rot = Quaternion.LookRotation(followLocation.position - caravan.transform.position);
				caravan.transform.rotation = Quaternion.Euler(caravan.transform.eulerAngles.x, rot.eulerAngles.y,
					caravan.transform.eulerAngles.z);
			}

			//room rotation
			if (room) {
				var roomTargetRot = Quaternion.Euler(-caravan.transform.localEulerAngles.x, 0,
					caravan.transform.localEulerAngles.z);
				room.transform.localRotation = Quaternion.Lerp(room.transform.rotation, roomTargetRot, 0.5f);
			}

			//player position
			if (_drivingPlayer) _drivingPlayer.transform.position = sitLocation.position;
		}

		private void FixedUpdate()
		{
			_rb.AddForce(-new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z) * 0.1f, ForceMode.VelocityChange);
		}

		private void OnDrawGizmos()
		{
			if (followLocation) {
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(followLocation.position, 0.5f);
			}
		}

		public IInteractable PickupOrDropObject(bool pickUp, Vector3 placePosition = default)
		{
			if (pickUp) {
				if (!_isDriven.Value) SetUprightPositionRpc();
			} else {
				StopDrivingCarRpc();
				if (_drivingPlayer) {
					_drivingPlayer.SetDriving(false);
					_drivingPlayer.SetCaravanControl(null);
					var right = followLocation.right;
					right.y = 0;
					right.Normalize();
					_drivingPlayer.transform.position = followLocation.position + right * 3f;
					_drivingPlayer = null;
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

		//driving
		public void Drive(Vector2 inputVector)
		{
			if (Vector3.Dot(transform.forward, caravan.transform.forward) > 0) {
				transform.Rotate(transform.up, inputVector.x * inputVector.y * 50f * Time.fixedDeltaTime);
			}

			SnailMovement(inputVector);
		}

		private void SnailMovement(Vector2 inputVector)
		{
			var forward = transform.forward;
			forward.y = 0;
			var moveVector = (forward * inputVector.y).normalized * (Speed * Time.fixedDeltaTime);

			if (_rb.linearVelocity.magnitude < Speed && inputVector.y > 0) {
				_rb.AddForce(moveVector, ForceMode.VelocityChange);
			}
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
			Drive(new Vector2(0, 0));
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
		private void SetUprightPositionRpc()
		{
			transform.rotation = Quaternion.identity;
		}
	}
}
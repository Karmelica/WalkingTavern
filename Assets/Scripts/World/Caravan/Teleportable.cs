using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace World.Caravan
{
	public class Teleportable : NetworkBehaviour
	{
		private NetworkTransform _trans;

		private void Awake()
		{
			_trans = GetComponent<NetworkTransform>();
		}

		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (!IsOwner) enabled = false;
		}

		public void Teleport(Vector3 position, Quaternion rotation)
		{
			if (!IsOwner) return;
			_trans.Teleport(position, rotation, Vector3.one);
			transform.SetPositionAndRotation(position, rotation);
			Physics.SyncTransforms();
		}
	}
}
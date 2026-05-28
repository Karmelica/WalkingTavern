using System;
using Netcode.Transports.Facepunch;
using Steamworks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Managers.Network.Steam
{
	public class SteamBootstrapper : MonoBehaviour
	{
		[SerializeField] private NetworkManager networkManager;
		[SerializeField] private FacepunchTransport facepunchTransport;
		[SerializeField] private UnityTransport unityTransport;
		
		private void Awake()
		{
			if (!SteamClient.IsValid) {
				networkManager.NetworkConfig.NetworkTransport = unityTransport;
			}
		}
	}
}
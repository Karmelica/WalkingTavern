using NaughtyAttributes;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
	public class LevelManager : NetworkBehaviour
	{
		[Scene] [SerializeField] private string nextScene;

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag("Player")) return;
			if (IsServer) NetworkManager.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
		}
	}
}
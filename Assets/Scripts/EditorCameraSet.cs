using Cooking.Minigames;
using UnityEngine;

[ExecuteAlways]
public class EditorCameraSet : MonoBehaviour
{
	[SerializeField] private Transform spawnLocation;
	private Minigame _minigame;
	private Transform cameraLocation;
	private Transform foodPlaceholder;

#if UNITY_EDITOR

	private void OnValidate()
	{
		if (!TryGetComponent(out _minigame)) return;
		cameraLocation = _minigame.cameraLocation;
		foodPlaceholder = _minigame.foodPlaceholder;
	}

	private void Update()
	{
		if (!_minigame) return;
		EditorUpdate();
	}

	private void EditorUpdate()
	{
		cameraLocation.LookAt(foodPlaceholder);
	}

	private void OnDrawGizmos()
	{
		if (spawnLocation) {
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(spawnLocation.position, 0.05f);
		}

		if (foodPlaceholder) {
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(foodPlaceholder.position, 0.05f);
		}

		if (cameraLocation) {
			Gizmos.color = Color.red;
			Gizmos.matrix = cameraLocation.localToWorldMatrix;
			Gizmos.DrawFrustum(Vector3.zero, 60, 0.3f, 200, 16 / 9f);
		}
	}
#endif
}
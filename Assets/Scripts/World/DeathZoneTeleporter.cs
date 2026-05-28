using UnityEngine;

public class DeathZoneTeleporter : MonoBehaviour
{
	private void OnTriggerStay(Collider other)
	{
		if (Physics.Raycast(other.transform.position, Vector3.up, out var hit, float.PositiveInfinity)) {
			other.transform.position = hit.point + Vector3.up;
		}
	}
}
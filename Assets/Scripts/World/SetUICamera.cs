using UnityEngine;

[DefaultExecutionOrder(100)]
public class SetUICamera : MonoBehaviour
{
	[SerializeField] private Canvas canvas;

	private Camera _camera;

	private void Start()
	{
		_camera = Camera.main;
		_camera = _camera?.GetComponentInChildren<Camera>();
		canvas.worldCamera = _camera;
	}
}
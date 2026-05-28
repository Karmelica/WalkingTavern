using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class FaceAnimation : MonoBehaviour
{
	private static readonly int FaceIndex = Shader.PropertyToID("_FaceIndex");
	[SerializeField] private float originalBlinkTimer = 7.5f;
	[SerializeField] private float blinkOffset = 2.5f;
	[SerializeField] private float blinkDuration = 0.2f;

	private float _blinkTimer;
	private float _currentTime;
	private SkinnedMeshRenderer _faceRenderer;

	private void Awake()
	{
		if (!TryGetComponent(out _faceRenderer)) enabled = false;
	}

	private void Update()
	{
#if UNITY_EDITOR
		EditorUpdate();
#else
        NormalUpdate();
#endif
	}

	private void NormalUpdate()
	{
		_currentTime += Time.deltaTime;
		if (_currentTime >= _blinkTimer + blinkDuration) {
			_currentTime = 0;
			_blinkTimer = originalBlinkTimer + Random.Range(-blinkOffset, blinkOffset);
			_faceRenderer.sharedMaterials[0].SetFloat(FaceIndex, Random.Range(1, 4));
		}

		if (_currentTime >= _blinkTimer) _faceRenderer.sharedMaterials[0].SetFloat(FaceIndex, 0);
	}

	private void EditorUpdate()
	{
		_currentTime += Time.deltaTime;
		if (_currentTime >= _blinkTimer + blinkDuration) {
			_currentTime = 0;
			_blinkTimer = originalBlinkTimer + Random.Range(-blinkOffset, blinkOffset);
			_faceRenderer.sharedMaterials[0].SetFloat(FaceIndex, Random.Range(1, 4));
		}

		if (_currentTime >= _blinkTimer) _faceRenderer.sharedMaterials[0].SetFloat(FaceIndex, 0);
	}
}
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace World.Caravan
{
	public class PortalCamera : MonoBehaviour
	{
		private const float NearClipLimit = 0.2f;
		private const float NearClipOffset = 0.05f;
		[SerializeField] private MeshRenderer portalRenderer;
		public PortalCamera otherPortal;
		private bool _isVisible;
		private Collider _objCollider;
		private Plane[] _planes;
		private Camera _playerCam;
		private Camera _portalCam;
		private RenderTexture _viewTexture;

		private void Start()
		{
			if (Camera.main == null) throw new Exception("MainCamera not found");
			_playerCam = Camera.main;
			_portalCam = GetComponentInChildren<Camera>();
			_portalCam.enabled = false;

			_portalCam.fieldOfView = _playerCam.fieldOfView;

			/*_viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
			_portalCam.targetTexture = _viewTexture;
			portalRenderer.material.mainTexture = _viewTexture;*/
		}

		private void LateUpdate()
		{
			Render();
		}


		private void OnDrawGizmosSelected()
		{
			if (!_portalCam) return;
			Gizmos.color = Color.blue;
			Gizmos.matrix = _portalCam.transform.localToWorldMatrix;

			var clipPlane = transform;
			var dot = Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCam.transform.position));

			var camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
			var camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
			var camSpaceDst = Vector3.Dot(camSpacePos, camSpaceNormal) + NearClipOffset;

			Gizmos.DrawFrustum(Vector3.zero, _portalCam.fieldOfView, camSpaceDst, _portalCam.farClipPlane, 16 / 9f);
		}

		private void OnValidate()
		{
			if (otherPortal) otherPortal.otherPortal = this;
		}

		private void CreateViewTexture()
		{
			if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height) {
				if (_viewTexture != null) _viewTexture.Release();
				_viewTexture = new RenderTexture(Screen.width, Screen.height, 24);

				// Render the view from the portal camera to the view texture
				_portalCam.targetTexture = _viewTexture;

				// Display the view texture on the screen of the linked portal
				otherPortal.portalRenderer.material.SetTexture("_MainTex", _viewTexture);
			}
		}

		public void Render()
		{
			if (!IsObjectVisible()) return;
			CreateViewTexture();

			var m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix *
			        _playerCam.transform.localToWorldMatrix;
			_portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);

			portalRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

			SetNearClipPlane();
			_portalCam.Render();

			portalRenderer.shadowCastingMode = ShadowCastingMode.On;
		}

		private void SetNearClipPlane()
		{
			var clipPlane = transform;
			var dot = Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCam.transform.position));

			var camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
			var camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
			var camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + NearClipOffset;

			if (Mathf.Abs(camSpaceDst) > NearClipLimit) {
				var clipPlaneCameraSpace =
					new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

				_portalCam.projectionMatrix = _playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
			} else {
				_portalCam.projectionMatrix = _playerCam.projectionMatrix;
			}
		}

		private bool IsObjectVisible()
		{
			_planes = GeometryUtility.CalculateFrustumPlanes(_playerCam);
			var meshRenderer = otherPortal.portalRenderer;
			return GeometryUtility.TestPlanesAABB(_planes, meshRenderer.bounds);
		}
	}
}
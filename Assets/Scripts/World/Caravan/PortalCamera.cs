using System;
using System.Collections.Generic;
using UnityEngine;

namespace World.Caravan
{
    public class PortalCamera : MonoBehaviour
    {
        private Camera _portalCam;
        [SerializeField] private MeshRenderer portalRenderer;
        private Camera _playerCam;
        private RenderTexture _viewTexture;
        public PortalCamera otherPortal;
        private const float NearClipLimit = 0.2f;
        private const float NearClipOffset = 0.05f;
        private Plane[] _planes;
        private Collider _objCollider;
        private bool _isVisible;

        void Start()
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

        private void OnValidate()
        {
            if (otherPortal) otherPortal.otherPortal = this;
        }

        private void LateUpdate()
        {
            Render();
        }

        private void CreateViewTexture ()
        {
            if (_viewTexture == null || _viewTexture.width != Screen.width || _viewTexture.height != Screen.height)
            {
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
            CreateViewTexture ();
            
            var m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * _playerCam.transform.localToWorldMatrix;
            _portalCam.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);
            
            portalRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            
            SetNearClipPlane();
            _portalCam.Render();
            
            portalRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
    
        void SetNearClipPlane () {
            Transform clipPlane = transform;
            int dot = Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - _portalCam.transform.position));

            Vector3 camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
            Vector3 camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + NearClipOffset;

            if (Mathf.Abs (camSpaceDst) > NearClipLimit) {
                Vector4 clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                _portalCam.projectionMatrix = _playerCam.CalculateObliqueMatrix (clipPlaneCameraSpace);
            }
            else {
                _portalCam.projectionMatrix = _playerCam.projectionMatrix;
            }
        }
        
        private bool IsObjectVisible()
        {
            _planes =  GeometryUtility.CalculateFrustumPlanes(_playerCam);
            var meshRenderer = otherPortal.portalRenderer;
            return GeometryUtility.TestPlanesAABB(_planes, meshRenderer.bounds);
        }


        private void OnDrawGizmosSelected()
        {
            if (!_portalCam) return;
            Gizmos.color = Color.blue;
            Gizmos.matrix = _portalCam.transform.localToWorldMatrix;
            
            Transform clipPlane = transform;
            int dot = Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - _portalCam.transform.position));

            Vector3 camSpacePos = _portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
            Vector3 camSpaceNormal = _portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
            float camSpaceDst = Vector3.Dot (camSpacePos, camSpaceNormal) + NearClipOffset;
            
            Gizmos.DrawFrustum(Vector3.zero, _portalCam.fieldOfView, camSpaceDst, _portalCam.farClipPlane, 16/9f);
        }
    }
}

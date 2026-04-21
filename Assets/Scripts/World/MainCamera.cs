using UnityEngine;
using UnityEngine.Rendering;
using World.Caravan;

namespace World
{
    public class MainCamera : MonoBehaviour
    {
        private PortalCamera[] _portalCameras;
    
        private void Awake()
        {
            _portalCameras = FindObjectsByType<PortalCamera>(FindObjectsSortMode.None);
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera contextCamera)
        {
            if (contextCamera != Camera.main) return;
            foreach (var portalCamera in _portalCameras)
            {
                portalCamera.Render();
            }
        }
    }
}

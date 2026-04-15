using UnityEngine;
using UnityEngine.Rendering;
using World.Caravan;

namespace World
{
    public class MainCamera : MonoBehaviour
    {
        private PortalCamera[] portalCameras;
    
        private void Awake()
        {
            portalCameras = FindObjectsByType<PortalCamera>(FindObjectsSortMode.None);
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera contextCamera)
        {
            if (contextCamera != Camera.main) return;
            foreach (var c in portalCameras)
            {
                c.Render();
            }
        }
    }
}

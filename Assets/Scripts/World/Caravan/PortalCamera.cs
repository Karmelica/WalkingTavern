using System;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{

    private MeshRenderer portalRenderer;
    private RenderTexture viewTexture;
    private Camera playerCam;
    public PortalCamera otherPortal;
    [SerializeField] private Camera portalCam;
    private float nearClipLimit = 0.1f;
    private float nearClipOffset = 0.03f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portalCam.enabled = false;
        
        if (Camera.main == null)
        {
            Debug.LogError("MainCamera not found");
            return;
        }
        playerCam = Camera.main;
        
        portalCam.fieldOfView = playerCam.fieldOfView;
        
        portalRenderer = GetComponentInChildren<MeshRenderer>();

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        portalCam.targetTexture = rt;
        portalRenderer.material.mainTexture = rt;
    }

    private void OnValidate()
    {
        if (otherPortal)
        {
            otherPortal.otherPortal = this;
        }
    }

    void CreateViewTexture () {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
            if (viewTexture != null) {
                viewTexture.Release ();
            }
            viewTexture = new RenderTexture (Screen.width, Screen.height, 24);
            
            // Render the view from the portal camera to the view texture
            portalCam.targetTexture = viewTexture;
            
            // Display the view texture on the screen of the linked portal
            otherPortal.portalRenderer.material.mainTexture = viewTexture;
        }
    }
    
    public void Render(){ 
     
        portalRenderer.enabled = false;
        CreateViewTexture ();
            
        var m = transform.localToWorldMatrix * otherPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;

        SetNearClipPlane();
        
        portalCam.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);
        
        portalCam.Render();

        portalRenderer.enabled = true;
    }
    
    void SetNearClipPlane () {
        Transform clipPlane = transform;
        int dot = System.Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + nearClipOffset;

        if (Mathf.Abs (camSpaceDst) > nearClipLimit) {
            Vector4 clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix (clipPlaneCameraSpace);
        } else 
        {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }
}

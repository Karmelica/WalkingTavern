using System;
using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Vector3 globalOffset { get; private set; }

    private MeshRenderer portalRenderer;
    private RenderTexture viewTexture;
    private Camera playerCam;
    public PortalCamera otherPortal;
    [SerializeField] private Camera portalCam;


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
        
        portalRenderer = GetComponent<MeshRenderer>();

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

        portalCam.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);

        globalOffset = transform.position - otherPortal.transform.position;

        portalCam.Render();

        portalRenderer.enabled = true;
    }
    
}

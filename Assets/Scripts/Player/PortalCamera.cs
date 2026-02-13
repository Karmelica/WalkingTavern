using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    private Camera mainCamera;
    private Camera portalCamera;
    [SerializeField] private Transform referenceTransform;
    [SerializeField] private Transform portalTransform;
    [SerializeField] private MeshRenderer portalRenderer;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portalCamera = GetComponent<Camera>();
        
        if (Camera.main == null)
        {
            Debug.LogError("Camera not found");
            return;
        }
        mainCamera = Camera.main;
        
        portalCamera.fieldOfView = mainCamera.fieldOfView;
        
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        portalCamera.targetTexture = rt;
        portalRenderer.material.mainTexture = rt;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.transform.position - referenceTransform.position +  portalTransform.position;
        transform.rotation = mainCamera.transform.rotation;
    }
}

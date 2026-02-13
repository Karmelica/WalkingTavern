using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    private MeshRenderer portalRenderer;
    private Camera mainCamera;
    [SerializeField] private Transform referenceTransform;
    [SerializeField] private Camera portalCamera;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("MainCamera not found");
            return;
        }
        mainCamera = Camera.main;
        portalCamera.fieldOfView = mainCamera.fieldOfView;
        
        portalRenderer = GetComponent<MeshRenderer>();
        
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        portalCamera.targetTexture = rt;
        portalRenderer.material.mainTexture = rt;
    }

    // Update is called once per frame
    void Update()
    {
        bool isVisible = portalRenderer.isVisible;
        
        portalCamera.gameObject.SetActive(isVisible);
        if (!isVisible) return;
        
        var offset = -transform.position;
        portalCamera.transform.position = mainCamera.transform.position + referenceTransform.position + offset;
        var distance = Vector3.Distance(portalCamera.transform.position, referenceTransform.position);
        portalCamera.nearClipPlane = Mathf.Clamp(distance - 2, 0.01f, 1000f);
        portalCamera.transform.rotation = mainCamera.transform.rotation;

    }
}

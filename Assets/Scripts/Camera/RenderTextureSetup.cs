using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureSetup : MonoBehaviour
{
    public Camera captureCam;
    public Camera renderCam;

    public RenderTexture rt;
    public GameObject renderPlane;

    public int textHeight = 216;

    private int prevWidth;
    private int prevHeight;

    void Start()
    {
        captureCam = gameObject.GetComponent<Camera>();

        // Creating Render Plane as a Child of RenderCam
        renderPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        renderPlane.transform.SetParent(renderCam.transform);
        renderPlane.transform.localPosition = new Vector3(0, 0, 10);
        renderPlane.transform.localRotation = Quaternion.Euler(90, 0, -180);

        RefreshRenderTexture();
    }

    void Update()
    {
        if (prevWidth != Screen.width || prevHeight != Screen.height) // Check if resolution has Changed
        {
            RefreshRenderTexture();
        }
    }

    void RefreshRenderTexture() // Refreshes the Render Texture - Changing size and resolution
    {
        // Create RenderTexture
        rt = new RenderTexture((int)(textHeight * renderCam.aspect), textHeight, 16, RenderTextureFormat.ARGB32);
        rt.filterMode = FilterMode.Point;
        rt.Create();

        captureCam.targetTexture = rt;

        renderPlane.GetComponent<Renderer>().material.mainTexture = rt;

        // Change size of Plane
        var scale = renderPlane.transform.localScale; 
        scale.x = scale.y * renderCam.aspect;
        renderPlane.transform.localScale = scale;

        prevWidth = Screen.width;
        prevHeight = Screen.height;
    }
}

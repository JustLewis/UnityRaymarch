using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayMarchSceneControl : MonoBehaviour
{
    private RawImage ImageHandler;

    private RenderTexture Map; //where the final data will end up.

    public ComputeShader CS;

    public Texture2D RedData;
    public Texture2D GreenData;
    public Texture2D BlueData;
    public Texture2D LuminsData;

    private int CSMain;

    private float MaxRed = 1.0f;
    private float MaxBlue = 1.0f;
    private float MaxGreen = 1.0f;
    private float MaxLumin = 1.0f;

    private float[] Offsets = new float[]{0.0f,0.0f,0.0f,0.0f,0.0f,0.0f};
    private uint ID = 0;
    private void Awake()
    {
        RectTransform RT = GetComponent<RectTransform>();
        RT.sizeDelta = new Vector2(Screen.width, Screen.height);

        ImageHandler = GetComponent<RawImage>();

        if (ImageHandler == null) Debug.LogError("No image handler");
        Map = new RenderTexture(Screen.width, Screen.height, 0);
        Map.enableRandomWrite = true;

        if (ImageHandler != null)
        {
            ImageHandler.texture = Map;
        }
        Debug.Log(RedData.width);
    }
    // Start is called before the first frame update
    void Start()
    {
        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);

        CS.SetTexture(CSMain, "RedData", RedData);
        CS.SetTexture(CSMain, "GreenData", GreenData);
        CS.SetTexture(CSMain, "BlueData", BlueData);
        CS.SetTexture(CSMain, "LuminData", LuminsData);

        CS.SetFloat("MaxRed", MaxRed);
        CS.SetFloat("MaxGreen", MaxGreen);
        CS.SetFloat("MaxBlue", MaxBlue);
        CS.SetFloat("MaxLumin", MaxLumin);

        dispatch();
    }

    void dispatch()
    {
        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
    }
}

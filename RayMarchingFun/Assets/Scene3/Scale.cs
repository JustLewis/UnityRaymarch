using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Scale : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;
    private Camera Cam;
    public Text ScreenText;
    public ComputeShader CS;
    public int RaySteps = 250;

    private int CSMain;

    //Game Variables
    private Vector3 PlayerInput = new Vector3(0f, 0f,-1.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.21f));

    private Vector4 SphereData = new Vector4(0, 0, 1.0f, 0.021f);
    private float SphereScale = 0.021f;
    private float SphereScaleMult = 1.15f;

    private void Awake()
    {
        Cam = GetComponentInParent<Camera>();
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
    }
    // Start is called before the first frame update
    void Start()
    {
        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);

        //initial Uniforms
        CS.SetFloats("Resolution", new float[] { Screen.width, Screen.height });
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, -1.0f, 0.0f });
        CS.SetInt("MaxRaySteps", RaySteps);
        CS.SetFloats("SphereData", DataHandler.PackAndRetrieveData(SphereData));

        SetUniforms();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxisRaw("Horizontal") != 0.0f)
        {
            SphereScale +=  Input.GetAxisRaw("Horizontal") * SphereScaleMult;
        }

        if (SphereScale < 0.021f)
        {
            SphereScale = 0.021f;
        }
        SphereData.y = SphereScale + 2.0f;
        SphereData.w = SphereScale;

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);

        ScreenText.text = SphereScale + "\n Ball";
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("SphereData", DataHandler.PackAndRetrieveData(SphereData));
        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
    }
}

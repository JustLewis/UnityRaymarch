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
    public Text ScreenTextb;
    public ComputeShader CS;
    public int RaySteps = 250;

    private int CSMain;

    //Game Variables
    private Vector3 PlayerInput = new Vector3(0f, 0f,-1.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.21f));

    private float SphereScale = 0.0000908f;
    private Vector4 SphereData = new Vector4(0, 0, 0.009f, 0.0f);
    private Vector4 ShaderSphereData = new Vector4(-0.5f, -0.5f, 0.009f, 0.25f);
   
    public float SphereScaleMult = 1.15f;
    private float RotAngle = -7.870f;
    private void Awake()
    {
        SphereData.w = SphereScale;
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
        Cursor.lockState = CursorLockMode.Locked;
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

        
        if(Input.GetKey(KeyCode.Mouse0))
        {
            RotAngle += Input.GetAxisRaw("Mouse X") * Time.deltaTime * 10.0f;
        }
        if(Input.GetKey(KeyCode.Mouse1))
        {
            ShaderSphereData.x += Input.GetAxisRaw("Mouse X") * Time.deltaTime;
            ShaderSphereData.y -= Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        }

        if (Input.GetAxisRaw("Horizontal") != 0.0f)
        {
            SphereScale +=  Input.GetAxisRaw("Horizontal") * (SphereScaleMult * Time.deltaTime * SphereScale);
        }

        if (SphereScale < 0.000000000908f)
        {
            SphereScale = 0.000000000908f;
        }
        if (SphereScale > 3000000)
        {
            SphereScale = 3000000.0f;
        }
        SphereData.y = SphereScale + 0.050f;
        SphereData.w = SphereScale;

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
        SetText();
        
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("SphereData", DataHandler.PackAndRetrieveData(SphereData));
        CS.SetFloats("ShaderSphereData", DataHandler.PackAndRetrieveData(ShaderSphereData));
        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
        CS.SetFloat("RotAngle", RotAngle);
    }
    private void SetText()
    {
        ScreenText.text = (SphereScale * 2).ToString() + " Meters";
        if(SphereScale > 1000000)
        {
            ScreenTextb.text = "Starts to break down after 2 million meter diameter.";
        }
        else
        {
            ScreenTextb.text = "";
        }
    }

}

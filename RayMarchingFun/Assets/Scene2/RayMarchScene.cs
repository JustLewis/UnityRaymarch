using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RayMarchScene : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;

    public ComputeShader CS;
    public ComputeBuffer CB; //currently holds player data

    private int CSMain;
    private CamControl Cam;

    //Game Variables
    private Vector3 PlayerInput = new Vector3(0f, 0f,-1.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));

    private void Awake()
    {
        Cam = new CamControl();
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
        CS.SetFloats("Resolution", new float[] { (float)Screen.width, (float)Screen.height });
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, 1.0f, 0.0f });
        CS.SetInt("MaxRaySteps", 250);

        SetUniforms();
    }

    // Update is called once per frame
    void Update()
    { 
        PlayerInput.x += Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        PlayerInput.y += Input.GetAxisRaw("Vertical") * Time.deltaTime;
        Cam.Position += PlayerInput;//Not great
        Cam.Direction = new Vector3(0, 0, -1f);

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
    }

    private void OnDisable()
    {
        //CB.Dispose();
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);

        CS.SetFloats("CamPos", DataHandler.PackAndRetrieveData(Cam.Position));
        CS.SetFloats("CamDir", DataHandler.PackAndRetrieveData(Cam.Direction));
        Cam.CalculateViewMatrix();
        CS.SetFloats("CamRot", Cam.Data);
    }
}

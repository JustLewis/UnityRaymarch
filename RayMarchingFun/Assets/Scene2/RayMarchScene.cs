using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RayMarchScene : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;
    private Camera Cam;

    public ComputeShader CS;
    public int RaySteps = 250;

    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));

    //player view Variables
    public float RotAngle = -7.0f;
    private Vector3 PlayerInput = new Vector3(0f, .50f, 2.0f);
    
    private Vector3 CurrentLocation = new Vector3(0, 0.0f, -0.10f);
    private float CurrentSmooth = -0.1f;//initial smoothness
    private int CurrentShape = 0; //0 elipse 1 torus 2 cube
    private Vector3 CurrentRadius = new Vector3(0.1f,0.1f,0.1f);

    public float MouseSpeedMultiplier = 1.0f;
    public float InputSpeedMultiplier = 1.0f;

    private List<float> SphereData = new List<float>();
    private List<float> CubeData = new List<float>();
    private List<float> TorusData = new List<float>();

    private int CSMain;

    private ComputeBuffer SphereBuffer;
    private ComputeBuffer TorusBuffer;
    private ComputeBuffer CubeBuffer;

    private int SphereCount;
    private int TorusCount;
    private int CubeCount;

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
        Cursor.lockState = CursorLockMode.Locked;

        SphereBuffer = new ComputeBuffer(20, sizeof(float) * 7); //pos,3Dradius,smoothness
        CubeBuffer = new ComputeBuffer(20, sizeof(float) * 7); //pos,3Dradius,smoothness
        TorusBuffer = new ComputeBuffer(20, sizeof(float) * 7); //pos,3Dradius,smoothness

        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);
        CS.SetBuffer(CSMain, "SphereData", SphereBuffer);
        CS.SetBuffer(CSMain, "CubeData", CubeBuffer);
        CS.SetBuffer(CSMain, "TorusData", TorusBuffer);

        //initial Uniforms
        CS.SetFloats("Resolution", new float[] { Screen.width, Screen.height });
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, -1.0f, 0.0f });
        CS.SetInt("MaxRaySteps", RaySteps);

        SetUniforms();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            CurrentSmooth += Input.GetAxisRaw("Mouse ScrollWheel");// * Time.deltaTime;
            CurrentRadius.x += Input.GetAxisRaw("Mouse X") * Time.deltaTime * MouseSpeedMultiplier;
            CurrentRadius.y += Input.GetAxisRaw("Mouse Y") * Time.deltaTime * MouseSpeedMultiplier;
            CurrentRadius.z += Input.GetAxisRaw("Vertical") * Time.deltaTime * MouseSpeedMultiplier;
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            CurrentLocation.x += Input.GetAxisRaw("Mouse X") * Time.deltaTime * MouseSpeedMultiplier;
            CurrentLocation.y += Input.GetAxisRaw("Mouse Y") * Time.deltaTime * MouseSpeedMultiplier;
            CurrentLocation.z += Input.GetAxisRaw("Vertical") * Time.deltaTime * InputSpeedMultiplier;
        }
        else //no mouse held. Allow movement
        { 
            RotAngle += Input.GetAxisRaw("Mouse X") * Time.deltaTime * 10.0f;
            PlayerInput.x -= Input.GetAxisRaw("Horizontal") * Time.deltaTime;
            PlayerInput.z -= Input.GetAxisRaw("Vertical") * Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            SetBuffers();
        }
        if(Input.GetKeyUp(KeyCode.E))
        {
            CurrentShape++;
            //only 3 shapes
            if(CurrentShape == 3)
            {
                CurrentShape = 0;
            }
        }

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
    }

    private void ResetUniforms()
    {
        CurrentLocation = new Vector3(0, 0.0f, -0.10f);
        CurrentSmooth = -0.1f;//initial smoothness
        CurrentShape = 0; //0 elipse 1 torus 2 cube
        CurrentRadius = new Vector3(0.1f,0.1f,0.1f);
    }


    private void SetBuffers()
    {
        if(CurrentShape == 0)
        {
            SphereData.Add(CurrentLocation.x);
            SphereData.Add(CurrentLocation.y);
            SphereData.Add(CurrentLocation.z);
            SphereData.Add(CurrentRadius.x);
            SphereData.Add(CurrentRadius.y);
            SphereData.Add(CurrentRadius.z);
            SphereData.Add(CurrentSmooth);
            SphereBuffer.SetData(SphereData.ToArray());
            SphereCount = SphereData.Count / 7;
        }
        if (CurrentShape == 1)
        {
            TorusData.Add(CurrentLocation.x);
            TorusData.Add(CurrentLocation.y);
            TorusData.Add(CurrentLocation.z);
            TorusData.Add(CurrentRadius.x);
            TorusData.Add(CurrentRadius.y);
            TorusData.Add(CurrentRadius.z);
            TorusData.Add(CurrentSmooth);
            TorusBuffer.SetData(TorusData.ToArray());
            TorusCount = TorusData.Count / 7;
        }
        if (CurrentShape == 2)
        {
            CubeData.Add(CurrentLocation.x);
            CubeData.Add(CurrentLocation.y);
            CubeData.Add(CurrentLocation.z);
            CubeData.Add(CurrentRadius.x);
            CubeData.Add(CurrentRadius.y);
            CubeData.Add(CurrentRadius.z);
            CubeData.Add(CurrentSmooth);
            CubeBuffer.SetData(CubeData.ToArray());
            CubeCount = CubeData.Count / 7;
        }
        ResetUniforms();
    }

    private void OnDisable()
    {
        SphereBuffer.Dispose();
        TorusBuffer.Dispose();
        CubeBuffer.Dispose();
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("RotAngle", RotAngle);

        CS.SetFloats("CurrentLocation", DataHandler.PackAndRetrieveData(CurrentLocation));
        CS.SetFloats("CurrentRadius", DataHandler.PackAndRetrieveData(CurrentRadius));
        CS.SetFloat("CurrentSmooth", CurrentSmooth);
        CS.SetInt("CurrentShape", CurrentShape);

        CS.SetInt("SphereCount", SphereCount);
        CS.SetInt("CubeCount", CubeCount);
        CS.SetInt("TorusCount", TorusCount);

        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
    }
}

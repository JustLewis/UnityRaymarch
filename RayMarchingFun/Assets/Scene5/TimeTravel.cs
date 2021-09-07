using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TimeTravel : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;
    private Camera Cam;

    public ComputeShader CS;
    private ComputeBuffer SphereBuffer; //currently holds player data
    private ComputeBuffer CubeBuffer; //currently holds player data
    private ComputeBuffer TorusBuffer; //currently holds player data

    public int SphereCount = 10;
    public int CubeCount = 10;
    public int TorusCount = 10;

    public int RaySteps = 250;

    private int CSMain;

    //Game Variables
    public float RotAngle = -7.0f;
    private Vector3 PlayerInput = new Vector3(0f, 0.50f,1.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));
    private float BlendAmount = 1.0f;

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

    void CreateAndSetBuffers()
    {
        float MaxX = 4.0f;
        float MaxBlend = 2.0f;
        SphereBuffer = new ComputeBuffer(SphereCount, sizeof(float) * 5); //xyz position, smoothness and radius
        List<float> Data = new List<float>();
        for (int i = 0; i < SphereCount; i ++)
        {
            Data.Add(Random.Range(-MaxX, MaxX));
            Data.Add(Random.Range(1.0f, -0.0f));
            Data.Add(Random.Range(1.0f, MaxX));
            Data.Add(Random.Range(-MaxBlend, MaxBlend)); // smoothness (negative cuts away from scene.)
            Data.Add(Random.Range(0.1f, 0.50f)); //radius
        }
        SphereBuffer.SetData(Data);

        Data.Clear();

        CubeBuffer = new ComputeBuffer(CubeCount, sizeof(float) * 5); //xyz position, smoothness and radius
        for (int i = 0; i < CubeCount; i++)
        {
            Data.Add(Random.Range(-MaxX, MaxX));
            Data.Add(Random.Range(1.0f, -0.0f));
            Data.Add(Random.Range(1.0f, MaxX));
            Data.Add(Random.Range(-MaxBlend, MaxBlend)); // smoothness (negative cuts away from scene.)
            Data.Add(Random.Range(0.1f, 0.50f)); //radius
        }
        CubeBuffer.SetData(Data);

        Data.Clear();

        TorusBuffer = new ComputeBuffer(TorusCount, sizeof(float) * 5); //xyz position, smoothness and radius
        for (int i = 0; i < TorusCount; i++)
        {
            Data.Add(Random.Range(-MaxX, MaxX));
            Data.Add(Random.Range(1.0f, 0.0f));
            Data.Add(Random.Range(1.0f, MaxX));
            Data.Add(Random.Range(-MaxBlend, MaxBlend)); // smoothness (negative cuts away from scene.)
            Data.Add(Random.Range(0.1f, 0.50f)); //radius
        }
        TorusBuffer.SetData(Data);

        Data.Clear();
        //SphereBuffer.GetData(Data.ToArray());
    }
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);

        CreateAndSetBuffers();

        CS.SetBuffer(CSMain, "SphereData", SphereBuffer);
        CS.SetBuffer(CSMain, "TorusData", TorusBuffer);
        CS.SetBuffer(CSMain, "CubeData", CubeBuffer);


        //initial Uniforms
        CS.SetFloats("Resolution", new float[] { Screen.width, Screen.height });
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, -1.0f, 0.0f });
        CS.SetInt("MaxRaySteps", RaySteps);
        CS.SetInt("TorusCount", TorusCount);
        CS.SetInt("SphereCount", SphereCount);
        CS.SetInt("CubeCount", CubeCount);

        SetUniforms();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.Mouse0))
        {
            RotAngle += Input.GetAxisRaw("Mouse X") * Time.deltaTime * 10.0f;
        }
        PlayerInput.x -= Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        PlayerInput.z -= Input.GetAxisRaw("Vertical") * Time.deltaTime;

        BlendAmount += Input.GetAxisRaw("Mouse ScrollWheel");

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
    }

    private void OnDisable()
    {
        SphereBuffer.Dispose();
        CubeBuffer.Dispose();
        TorusBuffer.Dispose();
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("RotAngle", RotAngle);
        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
        CS.SetFloat("BlendAmount", BlendAmount);
    }
}

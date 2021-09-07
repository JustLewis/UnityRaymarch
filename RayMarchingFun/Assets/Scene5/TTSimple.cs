using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TTSimple : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;
    private Camera Cam;

    public ComputeShader CS;

    public int RaySteps = 250;

    private int CSMain;

    //Game Variables
    public float RotAngle = -7.0f;
    private Vector3 PlayerInput = new Vector3(0f, 0.50f,1.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));
    float Gravityf = 1.0f;

    private ComputeBuffer SphereBuffer; //currently holds player data
    public int SphereCount = 16;

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

    void CreateBuffers()
    {
        float Pos = -10.0f;
        float DistAppart = 2.0f;
        float Radius = .50f;
        SphereBuffer = new ComputeBuffer(SphereCount, sizeof(float) * 4); //xyz position, smoothness and radius
        List<float> Data = new List<float>();
        for (int i = 0; i < SphereCount / 4; i++)
        {
            for (int j = 0; j < SphereCount / 4; j++)
            {
                Data.Add(Pos + (i * DistAppart)); //xpos
                Data.Add(Random.Range(1.0f, -0.0f)); //y pos
                Data.Add(Pos + (j * DistAppart));//ypos
                Data.Add(Radius);
            }
        }
        SphereBuffer.SetData(Data);
        Data.Clear(); //no need to keep array
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);

        CreateBuffers();

        CS.SetBuffer(CSMain, "SphereData", SphereBuffer);

        //initial Uniforms
        CS.SetFloats("Resolution", new float[] { Screen.width, Screen.height });
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, -1.0f, 0.0f });
        CS.SetInt("MaxRaySteps", RaySteps);

        CS.SetFloat("Gravityf", Gravityf);
        CS.SetFloats("BLPos", new float[] { 0.0f, -1.0f, 10.0f });
        CS.SetInt("SphereCount", SphereCount);

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

        Gravityf += Input.GetAxisRaw("Mouse ScrollWheel");
        if(Input.GetKey(KeyCode.E))
        {
            print("Gravityf = " + Gravityf);
        }

        SetUniforms();

        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
    }

    private void SetUniforms()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("RotAngle", RotAngle);
        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
        CS.SetFloat("Gravityf", Gravityf);
        //CS.SetFloat("BlendAmount", BlendAmount);
    }
    private void OnDisable()
    {
        SphereBuffer.Dispose();
    }
}

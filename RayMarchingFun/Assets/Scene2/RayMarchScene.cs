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
    public ComputeBuffer CB; //currently holds player data
    public int RaySteps = 250;

    private int CSMain;

    //Game Variables
    public float RotAngle = -7.0f;
    private Vector3 PlayerInput = new Vector3(0f, .50f,2.0f);
    private Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));
    private Vector3 SecondBall = new Vector3(0, 0.0f, -0.10f);

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

        CSMain = CS.FindKernel("CSMain");
        CS.SetTexture(CSMain, "MapTex", Map);

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
            RotAngle += Input.GetAxisRaw("Mouse X") * Time.deltaTime * 10.0f;
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            SecondBall.x += Input.GetAxisRaw("Mouse X") * Time.deltaTime;
            SecondBall.y += Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
        }
        SecondBall.z += Input.GetAxisRaw("Mouse ScrollWheel");// * Time.deltaTime;

        PlayerInput.x -= Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        PlayerInput.z -= Input.GetAxisRaw("Vertical") * Time.deltaTime;

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
        CS.SetFloats("PlayerInput", DataHandler.PackAndRetrieveData(PlayerInput));
        CS.SetFloats("SecondBall", DataHandler.PackAndRetrieveData(SecondBall));
        CS.SetFloats("RotAngle", RotAngle);
        CS.SetFloats("Time", Time.time);
        CS.SetInt("MaxRaySteps", RaySteps);
    }
}

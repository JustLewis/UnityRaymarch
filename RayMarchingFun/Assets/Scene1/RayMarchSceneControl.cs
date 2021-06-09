using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayMarchSceneControl : MonoBehaviour
{
    private RawImage ImageHandler;
    private RenderTexture Map;

    public ComputeShader CS;
    public ComputeBuffer CB; //currently holds player data

    private int CSMain, CSMainPhysics, CSMainPlayerPhysics;
    private float Speed = 0.0015f;
    public int NumberOfBalls = 5;

    //private int[] CBData = new int[] { 0, 0 };
    public float[] PlayerInput = new float[] { 0f, 0f };

    private List<float> Data = new List<float>();
    private float RotAngle = -7.48f;

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
    }
    // Start is called before the first frame update
    void Start()
    {
        CSMain = CS.FindKernel("CSMain");
        CSMainPhysics = CS.FindKernel("CSMainPhysics");
        CSMainPlayerPhysics = CS.FindKernel("CSMainPlayerPhysics");
        CS.SetTexture(CSMain, "MapTex", Map);

        Data.Add(0.0f);
        Data.Add(-1.0f); //inverted y 
        Data.Add(1.0f);
        Data.Add(0f);
        Data.Add(0f);
        Data.Add(0f);
        //radius
        Data.Add(0.5f);

        for (int i = 1; i < NumberOfBalls; i ++)
        {
            Data.Add(Random.Range(-8f, 8f));
            Data.Add(Random.Range(-3f, 0f)); //inverted y 
            Data.Add(Random.Range(2.0f, 8f)); //So not inside camera
            //velocities calculated on shader
            Data.Add(0f);
            Data.Add(0f);
            Data.Add(0f);
            //radius
            Data.Add(Random.Range(0.1f, 0.8f));
        }

        CB = new ComputeBuffer(NumberOfBalls, sizeof(float) * 7);
        CB.SetData(Data); //setting sphere data
        CS.SetBuffer(CSMain, "Player", CB);
        CS.SetBuffer(CSMainPhysics, "Player", CB);
        CS.SetBuffer(CSMainPlayerPhysics,"Player",CB);

        CS.SetFloats("Resolution", new float[] { (float)Screen.width, (float)Screen.height });
        Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, 1.0f, 0.0f });
        CS.SetFloats("Gravity", new float[] { 0, -10, 0 });
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetInt("MaxRaySteps", 250);
    }
    private void OnDisable()
    {
        CB.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            RotAngle += Input.GetAxisRaw("Mouse X") * Time.deltaTime * 10.0f;
        }

        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetInt("BallCount", NumberOfBalls);
        PlayerInput[0] = Input.GetAxisRaw("Horizontal") * Speed;
        PlayerInput[1] = Input.GetAxisRaw("Vertical") * Speed;
        CS.SetFloats("PlayerInput", PlayerInput);
        CS.SetFloats("RotAngle", RotAngle);
        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
        CS.Dispatch(CSMainPhysics, NumberOfBalls, 1, 1); //100 objects
        CS.Dispatch(CSMainPlayerPhysics, 1, 1, 1); //Player Object.

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Speed += Time.deltaTime * 0.51f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Speed -= Time.deltaTime * 0.51f;
        }
        Speed = Mathf.Clamp(Speed, 0f, 100f);
    }
}

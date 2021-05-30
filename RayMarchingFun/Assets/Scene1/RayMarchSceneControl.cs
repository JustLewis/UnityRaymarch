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

    private int CSMain, CSMainPhysics;
    private float Speed = 0.0015f;
    public int NumberOfBalls = 20;

    //private int[] CBData = new int[] { 0, 0 };
    public float[] PlayerInput = new float[] { 0f, 0f };

    private List<float> Data = new List<float>();

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
        CS.SetTexture(CSMain, "MapTex", Map);

        for(int i = 0; i < NumberOfBalls; i ++)
        {
            Data.Add(Random.Range(-8f, 8f));
            Data.Add(Random.Range(-3f, 0f));
            Data.Add(Random.Range(-8f, 8f));
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

        CS.SetFloats("Resolution", new float[] { (float)Screen.width, (float)Screen.height });
        Vector3 SunDir = Vector3.Normalize(new Vector3(0.8f, 0.4f, 0.42f));
        CS.SetFloats("SunDir", new float[] { SunDir.x, SunDir.y, SunDir.z });
        CS.SetFloats("SkyLight", new float[] { 0.0f, 1.0f, 0.0f });
        CS.SetFloats("Gravity", new float[] { Physics.gravity.x, -Physics.gravity.y, Physics.gravity.z });
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
        CS.SetFloat("Delta", Time.deltaTime);
        CS.SetInt("BallCount", NumberOfBalls);
        PlayerInput[0] = -Input.GetAxisRaw("Horizontal") * Speed;
        PlayerInput[1] = Input.GetAxisRaw("Vertical") * Speed;
        CS.SetFloats("PlayerInput", PlayerInput);
        float rotation = 10 * Input.mousePosition.x / Screen.width;
        CS.SetFloats("RotAngle", rotation);
        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
        CS.Dispatch(CSMainPhysics, NumberOfBalls, 1, 1); //100 objects

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

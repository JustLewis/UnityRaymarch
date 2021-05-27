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

    //private int[] CBData = new int[] { 0, 0 };
    public float[] PlayerInput = new float[] { 0f, 0f };

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

        CB = new ComputeBuffer(7, sizeof(float));
        CB.SetData(new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0.5f });
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

    // Update is called once per frame
    void Update()
    {
        CS.SetFloat("Delta", Time.deltaTime);
        PlayerInput[0] += Input.GetAxisRaw("Horizontal") * Speed;
        PlayerInput[1] += Input.GetAxisRaw("Vertical") * Speed;
        CS.SetFloats("PlayerInput", PlayerInput);
        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);
        CS.Dispatch(CSMainPhysics, 1, 1, 1);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            Speed += Time.deltaTime * 0.1f;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            Speed -= Time.deltaTime * 0.1f;
        }
        Speed = Mathf.Clamp(Speed, 0f, 1f);
    }
}

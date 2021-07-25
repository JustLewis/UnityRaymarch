using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayMarchSceneControl : MonoBehaviour
{
    //This is the target that the texture will be rendered on. It's called RawImage in the Scene.
    private RawImage ImageHandler;

    private RenderTexture Map; //where the final data will end up.

    //The compute shader responsible for combining the data into a texture
    public ComputeShader CS; 

    //Each set of data.
    public Texture2D RedData;
    public Texture2D GreenData;
    public Texture2D BlueData;
    public Texture2D LuminsData;

    //The name of the program in the compute shader.
    private int CSMain;

    //These floats are the maximum value found in the data.
        //these are set to 1 for now because the textures I'm using do have a maximum of one.
    private float MaxRed = 1.0f;
    private float MaxBlue = 1.0f;
    private float MaxGreen = 1.0f;
    private float MaxLumin = 1.0f;

    private void Awake()
    {
        //Transform the size of the raw image to be the same as our screen size.
        RectTransform RT = GetComponent<RectTransform>();
        RT.sizeDelta = new Vector2(Screen.width, Screen.height);

        //Find the RawImage component 
        ImageHandler = GetComponent<RawImage>();
        if (ImageHandler == null) Debug.LogError("No image handler");

        //Create render texture with screen width and height.
        Map = new RenderTexture(Screen.width, Screen.height, 0);
        Map.enableRandomWrite = true; //Needs random write so that the compute shader can write to it.

        if (ImageHandler != null)
        {
            //Assign the texture.
            ImageHandler.texture = Map;
        }

        //Debugging how big the data is.
        //Debug.Log(RedData.width); 
    }
    // Start is called before the first frame update
    void Start()
    {
        //CSMain is the program inside MainComputeShader. You can have multiple programs in one shader.
        CSMain = CS.FindKernel("CSMain");
        //Set the textures. These are in the Compute Shader, found by their string names (MapTex, RedData, etc...)
        CS.SetTexture(CSMain, "MapTex", Map); 
        CS.SetTexture(CSMain, "RedData", RedData);
        CS.SetTexture(CSMain, "GreenData", GreenData);
        CS.SetTexture(CSMain, "BlueData", BlueData);
        CS.SetTexture(CSMain, "LuminData", LuminsData);

        //Setting uniforms. You'll need these for adjusting additive,multiple and stuff.
        CS.SetFloat("MaxRed", MaxRed);
        CS.SetFloat("MaxGreen", MaxGreen);
        CS.SetFloat("MaxBlue", MaxBlue);
        CS.SetFloat("MaxLumin", MaxLumin);

        //Dispatch tells the compute shader to execute.
            //Only call this when updating data, uniforms or whatever. It's best to avoid calling this every tick.
        dispatch();
    }

    void dispatch()
    {
        //Compute shader dispatch functions are a bit confusing. See below.
        CS.Dispatch(CSMain, Screen.width / 8, Screen.height / 8, 1);

        /* Here we are telling the GPU to execute a number of work groups to complete the task.
         *      In the Compute Shader we can do something similar for the number of innvoations.
         *      Inside MainComputeShader.compute you'll see the below
         *      
                \/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/
            
                    [numthreads(8, 8, 1)]
                    void CSMain(uint3 id : SV_DispatchThreadID)

                /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\
         *      
         *      This is where the number of innvocations is being set. 
         *      
         * The number of workgroups and innvocations needs to be equal to the size of the data being calculated.
         * So this is basically telling the GPU how much data to compute.
         * 
         * I believe Unity automatically handles memory barries so if the size of the data is larger than the number of
         * available cores, it sould be absolutely fine. 
         * 
         * If you're ever unsure how to set this up. Just make sure the the number of workgroups are 
         * equal to he size of the data with:
         *      CS.Dispatch(CSMain, Data.x * Data.y, 1, 1);
         *      or
         *      CS.Dispatch(CSMain,Data.x,Data.y,1);
         * 
         * and set the invocations to [numthreads(1,1,1)] in the compute shader.
         * 
         * Just bear in mind that this is a realy basic overview.
         */
    }
}

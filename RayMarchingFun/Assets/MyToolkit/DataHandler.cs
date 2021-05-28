using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CamControl
{
    public Vector3 Position;
    public Vector3 Direction;
    public float[] Data;
    public enum DataPos : ushort
    {
        Right =0,
        Up=1,
        Forward =2,
        RotationMat = 0
    }

    public CamControl(Vector3 Dir)
    {
        Data = new float[9] {
            1f,0f,0f, //right
            0f,1f,0f, //up
            0f,0f,1f}; //Forward
    }
    public CamControl()
    {
        Data = new float[9] {
            1f,0f,0f, //right
            0f,1f,0f, //up
            0f,0f,1f}; //Forward
    }

    public void SetData(Vector3 Vec,DataPos EnumID)
    {
        Data[(ushort)EnumID * 3 + 0] = Vec.x;
        Data[(ushort)EnumID * 3 + 1] = Vec.y;
        Data[(ushort)EnumID * 3 + 2] = Vec.z;
    }
    public void SetData(float [] Array, int DataSize,DataPos StartPos)
    {
        for(int i = 0; i < DataSize; i ++)
        {
            Data[(ushort)StartPos * 3 + i] = Array[i];
        }
    }
    public Vector3 GetVector(DataPos EnumID)
    {
        return new Vector3(Data[(ushort)EnumID * 3 + 0],
            Data[(ushort)EnumID * 3 + 1],
            Data[(ushort)EnumID * 3 + 2]);
    }
    public void CalculateViewMatrix()
    {
        Vector3 Dir = Vector3.Normalize(Position - Direction);
        Vector3 up = GetVector(DataPos.Up);
        Vector3 right = Vector3.Normalize(Vector3.Cross(Dir, up));
        Vector3 UpCalc = Vector3.Cross(up, right);

        float[] Data = new float[]
        {
            right.x,right.y,right.z,
            UpCalc.x,UpCalc.y,UpCalc.z,
            Dir.x,Dir.y,Dir.z
        };
        //Debug.Log("right = " + right);
        //Debug.Log("UP = " + UpCalc);
        //Debug.Log("Forward = " + Dir);
        SetData(Data, Data.Length, DataPos.RotationMat);
    }
}
public class DataHandler
{
    static public float[] PackAndRetrieveData(Vector3[] Vectors)
    {
        List<float> Container = new List<float>();
        
        foreach(Vector3 v in Vectors)
        {
            Container.Add(v.x);
            Container.Add(v.y);
            Container.Add(v.z);
        }
        return Container.ToArray();
    }
    static public float[] PackAndRetrieveData(Vector3 Vin)
    {
        List<float> Container = new List<float>();

            Container.Add(Vin.x);
            Container.Add(Vin.y);
            Container.Add(Vin.z);
        return Container.ToArray();
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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


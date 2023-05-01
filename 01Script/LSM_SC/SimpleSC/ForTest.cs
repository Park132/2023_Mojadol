using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        short a = (short)-10;
        short b = (short)-1;
        int dummy = ((int)b & (int)ushort.MaxValue);
        dummy += ((int)(a)&(int)ushort.MaxValue)<<16;
        Debug.Log(Convert.ToString(a,2));
        Debug.Log(Convert.ToString(dummy,2));

        Debug.Log("b = " + (short)((int)(dummy) & (int)ushort.MaxValue));
        Debug.Log(  "short"+(short)((int)(dummy>>16)&(int)ushort.MaxValue));
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

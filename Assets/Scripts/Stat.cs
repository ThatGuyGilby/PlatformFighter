using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public float baseValue = 5;

    public float GetValue()
    {
        return baseValue;
    }
}

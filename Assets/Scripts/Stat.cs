using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public float baseValue = 5;

    /// <summary>
    /// Get the value of this stat.
    /// </summary>
    /// <returns>The total value of this stat.</returns>
    public float GetValue()
    {
        return baseValue;
    }
}
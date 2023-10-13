using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BarImplement : MonoBehaviour
{
    [SerializeField]
    protected bool underZero, overOne;

    public abstract void SetBar(float value);
}

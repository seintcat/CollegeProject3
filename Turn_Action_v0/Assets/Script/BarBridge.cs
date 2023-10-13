using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarBridge
{
    BarImplement usingBar;

    public BarBridge(BarImplement target)
    {
        this.usingBar = target;
    }

    public float barValue
    {
        set
        {
            if (usingBar != null)
                usingBar.SetBar(value);
        }
    }
}

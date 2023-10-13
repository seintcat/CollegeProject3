using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarOrb : BarImplement
{
    [SerializeField]
    RectTransform barRect;

    public override void SetBar(float value)
    {
        float val = value;
        if (!underZero && val < 0)
            val = 0;
        if (!overOne && val > 1)
            val = 1;

        barRect.localScale = new Vector3(val, val, 1);
    }
}

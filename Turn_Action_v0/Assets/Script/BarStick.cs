using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarStick : BarImplement
{
    [SerializeField]
    RectTransform barRect;
    [SerializeField]
    bool barIsX;

    public override void SetBar(float value)
    {
        float val = value;
        if (!underZero && val < 0)
            val = 0;
        if (!overOne && val > 1)
            val = 1;

        barRect.anchorMin = Vector2.zero;
        if (barIsX)
            barRect.anchorMax = new Vector2(val, 1);
        else
            barRect.anchorMax = new Vector2(1, val);

        barRect.offsetMin = Vector2.zero;
        barRect.offsetMax = Vector2.zero;
    }
}

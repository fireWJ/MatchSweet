using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearSameSweet : ClearedSweet {

    private ColorSweet.ColorType color;
    public ColorSweet.ColorType Color
    {
        get
        {
            return color;
        }

        set
        {
            color = value;
        }
    }

    public override void Clear()
    {
        base.Clear();
        sweet.gameManager.ClearSameSweets(color);
    }
}

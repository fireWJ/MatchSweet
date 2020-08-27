using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLineSweet : ClearedSweet {

    public bool isRow;
    public override void Clear()
    {
        base.Clear();
        if (isRow)
        {
            sweet.gameManager.ClearRowSweets(sweet.Y);
        }
        else
        {
            sweet.gameManager.ClearColumnSweets(sweet.X);
        }
    }
}

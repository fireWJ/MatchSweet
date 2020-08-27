using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSweet : MonoBehaviour {

    //分别对应不同精灵图片s
	public enum ColorType
    {
         BLUE,
         GREEN,
         PINK,
         PURPLE,
         RED,
         YELLOW,
         ANY,
         COUNT
    }
    private ColorType color;
    public ColorType Color
    {
        get
        {
            return color;
        }

        set
        {
            SetColor(value);
        }
    }
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType type;
        public  Sprite sprite;
    }
    private SpriteRenderer spriteRenderer;
    public ColorSprite[] colorSprites;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    public int NumColor
    {
        get
        {
            return colorSprites.Length;
        }
    }

   
    void Awake()
    {
        spriteRenderer = transform.Find("Sweet").GetComponent<SpriteRenderer>();
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for(int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].type))
            {
                colorSpriteDict.Add(colorSprites[i].type, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType type)
    {
        color = type;
        if (colorSpriteDict.ContainsKey(type))
        {
            spriteRenderer.sprite = colorSpriteDict[type];
        }
    }
}

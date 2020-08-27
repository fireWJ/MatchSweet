using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {

    private int x;
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            if (CanMove())
            {
                x = value;
            }   
        }
    }
    private int y;
    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (CanMove())
            {
                y = value;
            }      
        }
    }
    private GameManager.SweetType type;
    public GameManager.SweetType Type
    {
        get
        {
            return type;
        }
    }
    private MoveSweet moveSweetComponent;
    public MoveSweet MoveSweetComponent
    {
        get
        {
            return moveSweetComponent;
        }
    }

    private ColorSweet colorSweetComponent;
    public ColorSweet ColorSweetComponent
    {
        get
        {
            return colorSweetComponent;
        }
    }

    private ClearedSweet clearSweetComponent;
    public ClearedSweet ClearSweetComponent
    {
        get
        {
            return clearSweetComponent;
        }
    }


    [HideInInspector]
    public GameManager gameManager;

    void Awake()
    {
        moveSweetComponent = GetComponent<MoveSweet>();
        colorSweetComponent = GetComponent<ColorSweet>();
        clearSweetComponent = GetComponent<ClearedSweet>();
    }

    public void Init(int _x,int _y,GameManager _gameManager,GameManager.SweetType _type)
    {
        x = _x;
        y = _y;
        gameManager = _gameManager;
        type = _type;
    }

    public bool CanMove()
    {
        return moveSweetComponent != null;
    }

    public bool CanColor()
    {
        return colorSweetComponent != null;
    }

    public bool CanClear()
    {
        return clearSweetComponent != null;
    }
    public void OnMouseEnter()
    {
        gameManager.EnterSweet(this);
    }
   
    public void OnMouseDown()
    {
        gameManager.PressSweet(this);      
    }

    public void OnMouseUp()
    {
        gameManager.Release();
    }

}

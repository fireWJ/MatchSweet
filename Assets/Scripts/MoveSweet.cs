using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSweet : MonoBehaviour {

    private GameSweet gameSweet;
    public GameSweet GameSweet
    {
        get
        {
            return gameSweet;
        }
    }
    private IEnumerator moveCoroutine;

    void Awake()
    {
        gameSweet = GetComponent<GameSweet>();
    }
    /// <summary>
    /// 控制新携程的开启，以及旧携程的关闭，也就是此处要开始新携程之前必须判断有没有之前的携程仍在工作
    /// </summary>
    /// <param name="newX"></param>
    /// <param name="newY"></param>
    /// <param name="time"></param>
    public void Move(int newX,int newY,float time)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }
        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine);
    }

    public IEnumerator MoveCoroutine(int x,int y,float time)
    {
        gameSweet.X = x;
        gameSweet.Y = y;
        Vector3 startPos = gameSweet.transform.position;
        Vector3 endPos = gameSweet.gameManager.CorrectPosition(x, y);
        for(float t = 0; t < time; t += Time.deltaTime)
        {
            gameSweet.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;//等待一帧
        }

        gameSweet.transform.position = endPos;

    }
}

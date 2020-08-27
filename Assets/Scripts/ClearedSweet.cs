using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearedSweet : MonoBehaviour {

    public AnimationClip clearAnimation;
    private bool isClearing;//标志位，用来判断是否正在清除甜品
    public AudioClip destroyClip;
    public bool IsClearing
    {
        get
        {
            return isClearing;
        }
    }

    protected GameSweet sweet;
    private Animator animator;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }
    /// <summary>
    /// 清除物品
    /// </summary>
    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(clearAnimation.name);
            //玩家得分+1，播放完整声音
            GameManager.Instance.score++;
            AudioSource.PlayClipAtPoint(destroyClip, transform.position);
            yield return new WaitForSeconds(clearAnimation.length);
            Destroy(gameObject);
        }
    }
}

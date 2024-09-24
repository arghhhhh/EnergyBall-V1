using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerAnimation : MonoBehaviour
{
    public Animator animator;
    public string animationName = "Default";
    public float startDelay = 0f;
    void Awake()
    {
        animator.enabled = false;
    }
    void Start()
    {
        StartCoroutine(DelayAnimationStart(startDelay));
    }

    IEnumerator DelayAnimationStart(float time)
    {
        yield return new WaitForSeconds(time);
    
        animator.enabled = true;
    }
}

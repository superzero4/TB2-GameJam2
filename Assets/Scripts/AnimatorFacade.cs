using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFacade : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RuntimeAnimatorController[] _controllers;
    float lengthOfDeathClip;
    private const string X = "XDir";
    private const string Y = "YDir";
    private const string Death = "Death";
    private const string Shoot = "Shoot";
    private const string XShoot = "XShoot";
    private const string YShoot = "YShoot";
    private const string ReloadProgress = "Progress";
    private const string ReloadBool = "Reload";
    private const float FirstFrameOfLoop = 2f;
    private float EaseInOut = .2f;
    public RuntimeAnimatorController this[int i] => _controllers[i];


    private void Start()
    {

        lengthOfDeathClip = 5;
    }
    public void SetOrientation(float x, float y, string xKey = X, string yKey = Y)

    {
        _animator.SetFloat(xKey, x);
        _animator.SetFloat(yKey, y);
    }
    public void Kill()
    {
        _animator.SetTrigger(Death);
    }
    public void ShootToward(float x, float y)
    {
        ReloadAnimation(false);
        SetOrientation(x, y, XShoot, YShoot);
        _animator.SetTrigger(Shoot);
    }
    public void ReloadAnimation(bool value = false)
    {
        _animator.SetBool(ReloadBool, value);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress">Send >=1 to stop animation</param>
    public void ReloadAnimation(float progress)
    {
        float value;
        //We cycle beetwen frame 3 and 4 while progressing
        //Debug.Log("Progress ; " + progress);
        if (progress > EaseInOut && progress < 1 - EaseInOut)
        {
            value = FirstFrameOfLoop + progress * 5 % 2;
            //Debug.Log("Middle : " + value);
        }
        else
        {
            if (progress > 1 - EaseInOut)
            {
                progress = 1 - progress;
            }
            //We use the first 3 frames as first 3 and last 3
            value = progress * (FirstFrameOfLoop / EaseInOut);
            //Debug.Log("Middle : " + value);
        }
        value = (float)Mathf.FloorToInt(value) / lengthOfDeathClip;
        //Debug.Log("Final value , " + value);
        _animator.SetFloat(ReloadProgress, value);
    }
    public void PickAnimator(int index)
    {
        if (index < 0 || index >= _controllers.Length)
        {
            Debug.LogWarning("is out of bounds, check " + nameof(AnimatorFacade) + " object and add correspoding controllers, using 0 or modulus isntead");
            index = Mathf.Max(0, index);
            index %= _controllers.Length;
        }
        _animator.runtimeAnimatorController = _controllers[index];
    }
}

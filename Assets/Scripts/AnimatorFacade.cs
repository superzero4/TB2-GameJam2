using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorFacade : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RuntimeAnimatorController[] _controllers;
    [SerializeField] private AnimationCurve _frameOnProgress;
    float lengthOfDeathClip;
    private const string X = "XDir";
    private const string Y = "YDir";
    private const string Death = "Death";
    private const string Shoot = "Shoot";
    private const string XShoot = "XShoot";
    private const string YShoot = "YShoot";
    private const string ReloadProgress = "Progress";
    private float V = .2f;

    private void Start()
    {
        lengthOfDeathClip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
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
        SetOrientation(x, y, XShoot, YShoot);
        _animator.SetTrigger(Shoot);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="progress">Send >=1 to stop animation</param>
    public void ReloadAnimation(float progress)
    {
        float value = _frameOnProgress.Evaluate(progress) / lengthOfDeathClip;
        //We cycle beetwen frame 3 and 4 while progressing
        if (progress > V && progress < 1 - V)
            value = 3 + progress * 10 % 2;
        else
        {
            if (progress > 1 - V)
            {
                progress = 1 - V;
            }
            //We use the first 2 frames as first 2 and last 2
            value = progress * (1 / V) * 2;
        }
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

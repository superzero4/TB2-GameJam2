using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorFacade : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RuntimeAnimatorController[] _controllers;
    private const string X = "XDir";
    private const string Y = "YDir";
    private const string Death = "Death";
    private const string Shoot = "Shoot";
    public void SetOrientation(float x, float y)
    {
        _animator.SetFloat(X, x);
        _animator.SetFloat(Y, x);
    }
    public void Kill()
    {
        _animator.SetTrigger(Shoot);
    }
    public void ShootToward(float x,float y)
    {
        SetOrientation(x, y);
        _animator.SetTrigger(Shoot);
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

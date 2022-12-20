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
    private const string XShoot = "Shoot";
    private const string YShoot = "Shoot";
    public void SetOrientation(float x, float y,string xKey = X,string yKey=Y)
    {
        _animator.SetFloat(X, x);
        _animator.SetFloat(Y, y);
    }
    public void Kill()
    {
        _animator.SetTrigger(Death);
    }
    public void ShootToward(float x,float y)
    {
        SetOrientation(x, y,XShoot,YShoot);
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

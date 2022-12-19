using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorOverrideSelector : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private RuntimeAnimatorController[] _controllers;

    public void PickAnimator(int index)
    {
        if (index < 0 || index >= _controllers.Length)
        {
            Debug.LogWarning("is out of bounds, check " + nameof(AnimatorOverrideSelector) + " object and add correspoding controllers, using 0 or modulus isntead");
            index = Mathf.Max(0, index);
            index %= _controllers.Length;
        }
        _animator.runtimeAnimatorController = _controllers[index];
    }
}

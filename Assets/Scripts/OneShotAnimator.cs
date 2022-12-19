using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotAnimator : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(ChangeSkin());
    }
    private IEnumerator ChangeSkin()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            GetComponent<AnimatorOverrideSelector>().PickAnimator(Random.Range(0, 4));
        }

    }
}

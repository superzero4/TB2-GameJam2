using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class FeedbackFacade : MonoBehaviour
{
    public MMF_Player _hitFeedback;
    public MMF_Player _givenFeedback;
    public player _player;
    private void Awake()
    {
        _player.HitTaken += PlayFeedback;
        _player.HitGiven += ()=>_givenFeedback.PlayFeedbacks();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            _givenFeedback.PlayFeedbacks();
    }

    private void PlayFeedback()
    {
        _hitFeedback.PlayFeedbacks();
    }
}

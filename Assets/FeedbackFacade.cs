using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using Unity.Netcode;

public class FeedbackFacade : MonoBehaviour
{
    public MMF_Player _hitFeedback;
    public MMF_Player _givenFeedback;
    public bool _enbaleHitGiven = false;
    public player _player;
    private void Awake()
    {
        //_player.HitTaken += PlayFeedback;
        _player.HitTaken += PlayFeedbackClientRpc;
        if (_enbaleHitGiven)
            _player.HitGiven += ()=>_givenFeedback.PlayFeedbacks();
    }
    /*// Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            PlayFeedback(0);
    }*/

    private void PlayFeedback(ulong x)
    {
        _hitFeedback.PlayFeedbacks();
    }
    [ClientRpc]
    private void PlayFeedbackClientRpc(ulong targetClient)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClient)
            return;
        _hitFeedback.PlayFeedbacks();
    }
}

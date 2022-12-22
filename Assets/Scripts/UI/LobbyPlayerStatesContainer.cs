using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerStatesContainer : MonoBehaviour
{
    public static LobbyPlayerState[] _playersData;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

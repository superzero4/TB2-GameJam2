using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject _prefab;

    private void Awake()
    {
        SpawnPlayer(NetworkManager.Singleton.LocalClientId);
    }

    private void SpawnPlayer(ulong clientID)
    {
        var player = Instantiate(_prefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
    }
}

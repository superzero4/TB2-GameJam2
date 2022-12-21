using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;

    private List<player> _players = new List<player>();

    private AudioManager audioManager;

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.Stop("Musique");
        audioManager.Play("Game");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientID)
    {
        var player = Instantiate(prefab);
        _players.Add(player.GetComponent<player>());
        
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
    }
}

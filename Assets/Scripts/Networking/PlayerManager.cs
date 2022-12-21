using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private player prefab;

    private List<player> _players = new List<player>();

    private AudioManager audioManager;

    public player TopPlayer => _players.Aggregate((p1, p2) => p1.KillCount > p2.KillCount ? p1 : p2);

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
        player.HitGiven += OnHitGiven;

        _players.Add(player.GetComponent<player>());
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
    }
    void OnHitGiven()
    {
        int max = _players.Max(player => player.KillCount);
        foreach (var p in _players)
        {
            if (p.KillCount == max)
                p.GetComponentInChildren<PlayerUI>().SetCrowns();
            else
                p.GetComponentInChildren<PlayerUI>().UnsetCrowns();

        }
    }
}

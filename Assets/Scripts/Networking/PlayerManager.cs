using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private List<player> _players = new List<player>();
    public player this[ulong networkID] => _players.Find((p) => p.OwnerClientId == networkID);

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
        var playerGO = Instantiate(prefab, _spawnPoints[_players.Count].position, Quaternion.identity);
        player player = playerGO.GetComponent<player>();
        player.manager = this;
        _players.Add(player);

        playerGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);

        player.Died += OnPlayerDied;
    }
    
    void OnPlayerDied(player player)
    {
        _players.Remove(player);

        if (_players.Count == 1)
            Invoke(nameof(EndRound), 2f);
    }

    void EndRound()
    {
        ServerGameNetPortal.Instance.EndRound();
    }
}
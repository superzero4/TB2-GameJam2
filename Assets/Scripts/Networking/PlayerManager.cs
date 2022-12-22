using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private player prefab;
    [SerializeField] private List<Transform> _spawnPoints;
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
        int count = _players.Count;
        var player = Instantiate(prefab, _spawnPoints[count].position, Quaternion.identity);
        _players.Add(player);
        var info = LobbyUI.lobbyPlayers[count];
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        player.Died += OnPlayerDied;
        player.HitGiven += OnHitGiven;
        player.animator.PickAnimator(info.IsSpecialSkin ? 4 : count);
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

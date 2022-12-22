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
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        Debug.Log("Sending : " + LobbyPlayerStatesContainer._playersData[(int)localClientId].SkinIndex);
        SpawnPlayerServerRpc(localClientId, LobbyPlayerStatesContainer._playersData[(int)localClientId].SkinIndex);

        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        audioManager.Stop("Musique");
        audioManager.Play("Game");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong clientID,int skinIndex)
    {
        int count = (int)clientID;
        Debug.Log("For server, skin index is " +
        LobbyPlayerStatesContainer._playersData[count].SkinIndex);
        player player = Instantiate(prefab, _spawnPoints[count].position, Quaternion.identity);
        _players.Add(player);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        player.Died += OnPlayerDied;
        player.HitGiven += OnHitGiven;
        player.SkinSelectionClientRpc(clientID, LobbyPlayerStatesContainer._playersData);
        Debug.Log("Setting : " + skinIndex);
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

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private player prefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private static List<player> _players = new List<player>();
    public static player GetPlayer(ulong playerId) => _players.Find((p) => p.OwnerClientId == playerId);

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
    private void SpawnPlayerServerRpc(ulong clientID, int skinIndex)
    {

        int count = (int)clientID;
        player player = Instantiate(prefab, _spawnPoints[count].position, Quaternion.identity);
        _players.Add(player);
        player.manager = this;
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        player.Died += OnPlayerDied;
        player.HitGiven += OnHitGiven;
        player.SkinSelectionClientRpc(clientID, LobbyPlayerStatesContainer._playersData);
    }



    void OnPlayerDied(player player)
    {
        _players.Remove(player);
        player.GetComponent<NetworkObject>().Despawn(false);
        Destroy(player.gameObject);

        if (_players.Count == 1)
            Invoke(nameof(EndRound), 2f);
    }


    void EndRound()
    {
        ServerGameNetPortal.Instance.EndRound();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            EndRound();
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

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<Transform> _spawnPoints;

    private static List<player> _players = new List<player>();
    public static player GetPlayer(ulong playerId) => _players.Find((p) => p.OwnerClientId == playerId);

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
		player.GetComponent<NetworkObject>().Despawn(false);
		Destroy(player.gameObject);
		
		if (_players.Count == 1)
			Invoke(nameof(EndRound), 2f);
	}

	void EndRound()
	{
		ServerGameNetPortal.Instance.EndRound();
	}
}
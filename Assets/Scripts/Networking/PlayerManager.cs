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
        var playerGO = Instantiate(prefab);
		player player = playerGO.GetComponent<player>();
        
		_players.Add(player);

        playerGO.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID, true);
        
		player.Died += OnPlayerDied;
    }

	void OnPlayerDied(player player)
	{
		_players.Remove(player);
		
		if (_players.Count == 0)
			ServerGameNetPortal.Instance.EndRound();
	}
}
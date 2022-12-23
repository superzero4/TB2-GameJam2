using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class LobbyUI : NetworkBehaviour
{
    [Header("References")] [SerializeField]
    private LobbyPlayerCard[] lobbyPlayerCards;

    [SerializeField] private Sprite[] charThumbnails;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int minPlayer;
    LobbyPlayerStatesContainer _container;
    private NetworkList<LobbyPlayerState> lobbyPlayers;

    private void Awake()
    {
        lobbyPlayers = new NetworkList<LobbyPlayerState>(default, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        inputField.onSelect.AddListener(CopyToClipboard);
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            lobbyPlayers.OnListChanged += HandleLobbyPlayersStateChanged;
        }

        if (IsServer)
        {
            startGameButton.gameObject.SetActive(true);

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        inputField.text = PlayerPrefs.GetString("Code");
    }

    public void OnKonamiCode()
    {
        Debug.Log("Sending RPC with " + NetworkManager.Singleton.LocalClientId);
        SpecialSkinServerRPC(NetworkManager.Singleton.LocalClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpecialSkinServerRPC(ulong clientID)
    {
        Debug.Log("Reiceved RPC with " + clientID);
        var p = lobbyPlayers[(int)clientID];
        p.IsSpecialSkin = true;
        lobbyPlayers[(int)clientID] = p;
        //Will call on list changed and update image
        //lobbyPlayerCards[p.ClientId].UpdateImage(charThumbnails[4]);
    }

    public override void OnDestroy()
    {
        LobbyPlayerStatesContainer._playersData = new LobbyPlayerState[lobbyPlayers.Count];
        int i = 0;
        foreach (var p in lobbyPlayers)
        {
            LobbyPlayerStatesContainer._playersData[i] = p;
            i++;
        }

        base.OnDestroy();

        lobbyPlayers.OnListChanged -= HandleLobbyPlayersStateChanged;

        if (NetworkManager.Singleton)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    private bool IsEveryoneReady()
    {
        if (lobbyPlayers.Count < minPlayer)
        {
            return false;
        }

        foreach (var player in lobbyPlayers)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    private void HandleClientConnected(ulong clientId)
    {
        var playerData = ServerGameNetPortal.Instance.GetPlayerData(clientId);

        if (!playerData.HasValue)
        {
            return;
        }

        lobbyPlayers.Add(new LobbyPlayerState(
            clientId,
            playerData.Value.PlayerName,
            0,
            false
        ));
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].ClientId == clientId)
            {
                lobbyPlayers.RemoveAt(i);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < lobbyPlayers.Count; i++)
        {
            if (lobbyPlayers[i].ClientId == serverRpcParams.Receive.SenderClientId)
            {
                var player = lobbyPlayers[i];
                player.IsReady = !player.IsReady;
                lobbyPlayers[i] = player;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (!IsEveryoneReady())
        {
            return;
        }

        ServerGameNetPortal.Instance.StartGame();
    }

    public void OnLeaveClicked()
    {
        GameNetPortal.Instance.RequestDisconnect();
    }

    public void OnReadyClicked()
    {
        ToggleReadyServerRpc();
    }

    public void OnStartGameClicked()
    {
        StartGameServerRpc();
    }

    private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> lobbyState)
    {
        Debug.Log("Occured on " + NetworkManager.Singleton.LocalClientId);
        for (int i = 0; i < lobbyPlayerCards.Length; i++)
        {
            if (lobbyPlayers.Count > i)
            {
                var lobbyPlayerState = lobbyPlayers[i];
                lobbyPlayerCards[i].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);

                if (lobbyPlayerState.ClientId == NetworkManager.Singleton.LocalClientId)
                {
                    lobbyPlayerCards[i].RightBtn.onClick.RemoveAllListeners();
                    lobbyPlayerCards[i].RightBtn.onClick
                        .AddListener(() => OnNextSkinServerRpc((int)lobbyPlayerState.ClientId));

                    lobbyPlayerCards[i].LeftBtn.onClick.RemoveAllListeners();
                    lobbyPlayerCards[i].LeftBtn.onClick
                        .AddListener(() => OnPreviousSkin((int)lobbyPlayerState.ClientId));
                }
                else
                {
                    lobbyPlayerCards[i].RightBtn.gameObject.SetActive(false);
                    lobbyPlayerCards[i].LeftBtn.gameObject.SetActive(false);
                }

                //AddListenerSkinServerRpc(lobbyPlayers[i], i);
            }
            else
            {
                lobbyPlayerCards[i].DisableDisplay();
            }
        }

        if (IsHost)
        {
            startGameButton.interactable = IsEveryoneReady();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddListenerSkinServerRpc(LobbyPlayerState lobbyPlayerState, int clientIndex)
    {
        lobbyPlayerCards[clientIndex].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);

        if (IsHost)
        {
            if (lobbyPlayerState.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                lobbyPlayerCards[clientIndex].RightBtn.onClick.RemoveAllListeners();
                lobbyPlayerCards[clientIndex].RightBtn.onClick
                    .AddListener(() => OnNextSkinServerRpc((int)lobbyPlayerState.ClientId));

                lobbyPlayerCards[clientIndex].LeftBtn.onClick.RemoveAllListeners();
                lobbyPlayerCards[clientIndex].LeftBtn.onClick
                    .AddListener(() => OnPreviousSkin((int)lobbyPlayerState.ClientId));
            }
            else
            {
                lobbyPlayerCards[clientIndex].RightBtn.gameObject.SetActive(false);
                lobbyPlayerCards[clientIndex].LeftBtn.gameObject.SetActive(false);
            }
        }

        AddListenerSkinClientRpc(lobbyPlayerState, clientIndex);
    }

    [ClientRpc]
    private void AddListenerSkinClientRpc(LobbyPlayerState lobbyPlayerState, int clientIndex)
    {
        if (lobbyPlayerState.ClientId == NetworkManager.Singleton.LocalClientId)
        {
            lobbyPlayerCards[clientIndex].RightBtn.onClick.RemoveAllListeners();
            lobbyPlayerCards[clientIndex].RightBtn.onClick
                .AddListener(() => OnNextSkinServerRpc((int)lobbyPlayerState.ClientId));

            lobbyPlayerCards[clientIndex].LeftBtn.onClick.RemoveAllListeners();
            lobbyPlayerCards[clientIndex].LeftBtn.onClick
                .AddListener(() => OnPreviousSkin((int)lobbyPlayerState.ClientId));
        }
        else
        {
            lobbyPlayerCards[clientIndex].RightBtn.gameObject.SetActive(false);
            lobbyPlayerCards[clientIndex].LeftBtn.gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnNextSkinServerRpc(int clientId)
    {
        if (IsHost)
        {
            LobbyPlayerState lobbyPlayerState = lobbyPlayers[clientId];

            if (lobbyPlayerState.IsSpecialSkin)
            {
                if (lobbyPlayerState.Skin + 1 > charThumbnails.Length - 1)
                    lobbyPlayerState.Skin = 0;
                else
                {
                    lobbyPlayerState.Skin++;
                }
            }
            else if (lobbyPlayerState.Skin + 1 > charThumbnails.Length - 2)
            {
                lobbyPlayerState.Skin = 0;
            }
            else
            {
                lobbyPlayerState.Skin++;
            }

            lobbyPlayers[clientId] = lobbyPlayerState;
            lobbyPlayerCards[clientId].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);
        }
        else
        {
            OnNextSkinClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void OnNextSkinClientRpc(int clientId)
    {
        LobbyPlayerState lobbyPlayerState = lobbyPlayers[clientId];

        if (lobbyPlayerState.IsSpecialSkin)
        {
            if (lobbyPlayerState.Skin + 1 > charThumbnails.Length - 1)
                lobbyPlayerState.Skin = 0;
            else
            {
                lobbyPlayerState.Skin++;
            }
        }
        else if (lobbyPlayerState.Skin + 1 > charThumbnails.Length - 2)
        {
            lobbyPlayerState.Skin = 0;
        }
        else
        {
            lobbyPlayerState.Skin++;
        }

        lobbyPlayers[clientId] = lobbyPlayerState;
        lobbyPlayerCards[clientId].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);
    }

    public void OnPreviousSkin(int clientId)
    {
        if (IsHost)
        {
            LobbyPlayerState lobbyPlayerState = lobbyPlayers[clientId];


            if (lobbyPlayerState.Skin - 1 < 0)
                lobbyPlayerState.Skin =
                    lobbyPlayerState.IsSpecialSkin ? charThumbnails.Length - 1 : charThumbnails.Length - 2;
            else
                lobbyPlayerState.Skin--;

            lobbyPlayers[clientId] = lobbyPlayerState;
            lobbyPlayerCards[clientId].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);
        }
        else
        {
            OnPreviousSkinClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void OnPreviousSkinClientRpc(int clientId)
    {
        LobbyPlayerState lobbyPlayerState = lobbyPlayers[clientId];


        if (lobbyPlayerState.Skin - 1 < 0)
            lobbyPlayerState.Skin =
                lobbyPlayerState.IsSpecialSkin ? charThumbnails.Length - 1 : charThumbnails.Length - 2;
        else
            lobbyPlayerState.Skin--;

        lobbyPlayers[clientId] = lobbyPlayerState;
        lobbyPlayerCards[clientId].UpdateDisplay(lobbyPlayerState, charThumbnails[lobbyPlayerState.SkinIndex]);
    }

    private static void CopyToClipboard(string str)
    {
        GUIUtility.systemCopyBuffer = str;
        Debug.Log("Copy to clipboard : " + GUIUtility.systemCopyBuffer);
    }
}
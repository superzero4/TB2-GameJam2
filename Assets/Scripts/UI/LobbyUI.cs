using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : NetworkBehaviour
{
    [Header("References")] [SerializeField]
    private LobbyPlayerCard[] lobbyPlayerCards;
    [SerializeField] private Sprite[] charThumbnails;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private int minPlayer;

    private NetworkList<LobbyPlayerState> lobbyPlayers;

    private void Awake()
    {
        lobbyPlayers = new NetworkList<LobbyPlayerState>();
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
        text.text = NetworkManager.Singleton.LocalClientId.ToString();
    }

    public override void OnDestroy()
    {
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
                lobbyPlayers[i] = new LobbyPlayerState(
                    lobbyPlayers[i].ClientId,
                    lobbyPlayers[i].PlayerName,
                    !lobbyPlayers[i].IsReady
                );
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
        for (int i = 0; i < lobbyPlayerCards.Length; i++)
        {
            if (lobbyPlayers.Count > i)
            {
                lobbyPlayerCards[i].UpdateDisplay(lobbyPlayers[i]);
                lobbyPlayerCards[i].UpdateImage(charThumbnails[i]);
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

    private static void CopyToClipboard(string str)
    {
        GUIUtility.systemCopyBuffer = str;
        Debug.Log("Copy to clipboard : " + GUIUtility.systemCopyBuffer);
    }
}
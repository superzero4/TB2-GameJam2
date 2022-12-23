using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour
{
    [Header("Panels")] [SerializeField] private GameObject waitingForPlayerPanel;
    [SerializeField] private GameObject playerDataPanel;

    [Header("Data Display")] [SerializeField]
    private TMP_Text playerDisplayNameText;

    [SerializeField] private Image selectedCharacterImage;
    [SerializeField] private Toggle isReadyToggle;
    public Button RightBtn;
    public Button LeftBtn;

    public void UpdateDisplay(LobbyPlayerState lobbyPlayerState,Sprite image)
    {
        playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
        isReadyToggle.isOn = lobbyPlayerState.IsReady;

        RightBtn.interactable = LeftBtn.interactable = !lobbyPlayerState.IsReady;

        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
        UpdateImageServerRpc(image);
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void UpdateImageServerRpc(Sprite sprite)
    {
        UpdateImageClientRpc(sprite);
    }

    [ClientRpc]
    private void UpdateImageClientRpc(Sprite sprite)
    {
        selectedCharacterImage.sprite = sprite;
    }
    
}
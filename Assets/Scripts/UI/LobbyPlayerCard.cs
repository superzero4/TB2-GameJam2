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
    [SerializeField] private Image readyImage;
    [SerializeField] private Toggle isReadyToggle;
    public Button RightBtn;
    public Button LeftBtn;

    public void UpdateDisplay(LobbyPlayerState lobbyPlayerState,Sprite image)
    {
        playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
        //isReadyToggle.isOn = lobbyPlayerState.IsReady;
        readyImage.color = lobbyPlayerState.IsReady ? Color.green : Color.red;
        RightBtn.interactable = LeftBtn.interactable = !lobbyPlayerState.IsReady;

        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
        
        selectedCharacterImage.sprite = image;
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
}
using TMPro;
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

    public void UpdateDisplay(LobbyPlayerState lobbyPlayerState,Sprite image)
    {
        playerDisplayNameText.text = lobbyPlayerState.PlayerName.ToString();
        isReadyToggle.isOn = lobbyPlayerState.IsReady;

        waitingForPlayerPanel.SetActive(false);
        playerDataPanel.SetActive(true);
        UpdateImage(image);
    }

    public void DisableDisplay()
    {
        waitingForPlayerPanel.SetActive(true);
        playerDataPanel.SetActive(false);
    }
    public void UpdateImage(Sprite sprite)
    {
        selectedCharacterImage.sprite = sprite;
    }
}
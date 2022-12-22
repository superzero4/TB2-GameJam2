using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField displayNameInputField;
    [SerializeField] private TMP_InputField displayCodeInputField;
    [SerializeField] private AudioManager audioManager;

    private void Start()
    {
        
        PlayerPrefs.GetString("PlayerName");
        StartRelay();
    }

    private async void StartRelay()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        audioManager.Play("Musique");
    }

    public void OnHostClicked()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);

        CreateRelay();
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnClientClicked()
    {
        PlayerPrefs.SetString("PlayerName", displayNameInputField.text);

        if (displayCodeInputField.text == null)
            return;

        JoinRelay(displayCodeInputField.text);
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode);
            PlayerPrefs.SetString("Code", joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            GameNetPortal.Instance.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            PlayerPrefs.SetString("Code", joinCode);

            Debug.Log("Try to join relay with code " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            ClientGameNetPortal.Instance.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
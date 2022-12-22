using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : NetworkBehaviour
{
	[SerializeField] float _speed;

	[SerializeField] Transform _spawnPoint;
	[SerializeField] List<Transform> _groundPoints;
	[SerializeField] List<Transform> _bannerPoints;
	
	[SerializeField] GameObject _playerPrefab;
	
	[SerializeField] List<BannerUI> _banners;
	[SerializeField] Button _playAgainButton;
	[SerializeField] Button _quitButton;
	[SerializeField] CanvasGroup _buttonsCanvasGroup;

	readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(1);
	
	IEnumerator Start()
	{
		if (!IsServer)
			yield break;

		var orderedPlayers = LobbyPlayerStatesContainer._playersData.OrderByDescending(data => data.KillCount).ToList(); 
		
		for (int i = 0; i < LobbyPlayerStatesContainer._playersData.Length; i++)
		{
			SpawnCharacterClientRPC(orderedPlayers[i], LobbyPlayerStatesContainer._playersData.Length, i);

			yield return _waitForSeconds;
		}
	}

	[ClientRpc]
	void SpawnCharacterClientRPC(LobbyPlayerState lobbyPlayerState, int playerCount, int index)
	{
		var player = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
		AnimatorFacade animatorFacade = player.GetComponent<AnimatorFacade>();
		animatorFacade.PickAnimator(lobbyPlayerState.SkinIndex);
	    _banners[index].SetData(lobbyPlayerState);
		animatorFacade.SetOrientation(1, 0);		
		StartCoroutine(Move(player.transform, _groundPoints[index].position,
			() =>
			{
				StartCoroutine(Move(player.transform, _bannerPoints[index].position, () =>
				{
					StartCoroutine(_banners[index].Show());

					if (index == playerCount - 1)
					{
						animatorFacade.Kill();
						_buttonsCanvasGroup.alpha = 1f;
						_quitButton.interactable = true;
						
						if (IsServer)
							_playAgainButton.interactable = true;
					}
					else
					{
						animatorFacade.SetOrientation(0, -1);
					}
				}));

				animatorFacade.SetOrientation(0, index != playerCount - 1 ? 1 : -1);
			}));
	}

	public void PlayAgain()
	{
		if (!IsServer)
			return;
		
		ServerGameNetPortal.Instance.PlayAgain();
	}

	public void Quit()
	{
		GameNetPortal.Instance.RequestDisconnect();
	}

	IEnumerator Move(Transform player, Vector3 to, Action onCompleted)
	{
		while (Vector3.Distance(player.position, to) > 0.1f)
		{
			player.position = Vector3.MoveTowards(player.position, to, _speed * Time.deltaTime);
			
			yield return null;
		}
		
		onCompleted.Invoke();
	}
}
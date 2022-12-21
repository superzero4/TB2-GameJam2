using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResultManager : MonoBehaviour
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
	
	List<PlayerData> _playersData;

	void Awake()
	{
		_playersData = new List<PlayerData>
		{
			new PlayerData("Krantt", 0),
			new PlayerData("bbbbbb", 1),
			new PlayerData("ttnarK", 2),
			new PlayerData("Pas Krantt", 3),
		};
		
		/*foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
		{
			PlayerData? playerData = ServerGameNetPortal.Instance.GetPlayerData(client.ClientId);

			if (playerData is null)
				continue;
			
			_playersData.Add(playerData.Value);
		}*/
	}

	IEnumerator Start()
	{
		for (int i = 0; i < _playersData.Count; i++)
		{
			var index = i;
			
			GameObject player = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
			AnimatorFacade animatorFacade = player.GetComponent<AnimatorFacade>();
			Animator animator = player.GetComponent<Animator>();

			_banners[index].SetData(_playersData[index]);
			animatorFacade.SetOrientation(1, 0);

			StartCoroutine(Move(player.transform, _groundPoints[i].position, 
				() =>
				{
					StartCoroutine(Move(player.transform, _bannerPoints[index].position, () =>
					{
						StartCoroutine(_banners[index].Show());

						if (index == _playersData.Count - 1)
						{
							animatorFacade.Kill();
							_buttonsCanvasGroup.alpha = 1f;
							_playAgainButton.interactable = true;
							_quitButton.interactable = true;
						}
						else
						{
							animatorFacade.SetOrientation(0, -1);
						}
					}));
					
					animatorFacade.SetOrientation(0, index != _playersData.Count - 1 ? 1 : -1);
				}));

			yield return _waitForSeconds;
		}
	}

	public void PlayAgain()
	{
		Debug.Log("Play again !");
	}

	public void Quit()
	{
		Debug.Log("Quit !");
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
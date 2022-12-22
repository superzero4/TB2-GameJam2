using System.Collections;
using TMPro;
using UnityEngine;

public class BannerUI : MonoBehaviour
{
	[SerializeField] float _showDuration;
	
	[SerializeField] CanvasGroup _canvasGroup;
	
	[SerializeField] TextMeshProUGUI _nameText;
	[SerializeField] TextMeshProUGUI _scoreText;
	
	public void SetData(LobbyPlayerState lobbyPlayerState)
	{
		_nameText.text = lobbyPlayerState.PlayerName.Value;
		_scoreText.text = $"{lobbyPlayerState.KillCount}";
	}

	public IEnumerator Show()
	{
		float timer = 0f;

		while (timer <= _showDuration)
		{
			_canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / _showDuration);
			
			timer += Time.deltaTime;

			yield return null;
		}

		_canvasGroup.alpha = 1f;
	}
}
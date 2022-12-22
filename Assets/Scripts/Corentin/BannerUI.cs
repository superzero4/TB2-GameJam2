using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BannerUI : MonoBehaviour
{
	[SerializeField] float _showDuration;
	
	[SerializeField] CanvasGroup _canvasGroup;
	
	[SerializeField] TextMeshProUGUI _nameText;
	[SerializeField] TextMeshProUGUI _scoreText;
	
	public void SetData(PlayerData playerData)
	{
		_nameText.text = playerData.PlayerName;
		//_scoreText.text = $"{playerData.KillCount}";
	}
	
	public void SetData(string playerName)
	{
		_nameText.text = playerName;
		//_scoreText.text = $"{playerData.KillCount}";
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
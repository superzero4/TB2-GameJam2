using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[SerializeField] float _refillDuration;

	[SerializeField] List<Image> _heartsImage;
	[SerializeField] TextMeshProUGUI _scoreText;
	[SerializeField] Image _snowballFillImage;

	void OnEnable()
	{
		// player.HitTaken += OnHitTaken;
		// player.HitGiven += OnHitGiven;
		// player.SnowballThrown += OnSnowballThrown;
		// player.RefillSnowball += OnRefillSnowball;
	}

	void OnDisable()
	{
		// player.HitTaken -= OnHitTaken;
		// player.HitGiven -= OnHitGiven;
		// player.SnowballThrown -= OnSnowballThrown;
		// player.RefillSnowball -= OnRefillSnowball;
	}

	public void OnHitTaken()
	{
		// _heartsImage[^player.Health].color = Color.gray;
	}
	
	public void OnHitGiven()
	{
		// _scoreText.text = $"{player.KillCount}";
	}
	
	public void OnSnowballThrown()
	{
		_snowballFillImage.fillAmount = 0f;
	}

	public void OnRefillSnowball()
	{
		StartCoroutine(RefillSnowball());
	}
	
	IEnumerator RefillSnowball()
	{
		float timer = 0;

		while (timer <= _refillDuration)
		{
			_snowballFillImage.fillAmount = Mathf.Lerp(0f, 1f, timer / _refillDuration);

			timer += Time.deltaTime;
			
			yield return null;
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[SerializeField] player _player;

	[SerializeField] Image _crownImage;
	[SerializeField] List<Image> _heartsImage;
	[SerializeField] TextMeshProUGUI _scoreText;
	[SerializeField] Image _snowballFillImage;

	Stack<Image> _activeHearts;

	void Awake()
	{
		_activeHearts = new Stack<Image>(_heartsImage);
	}

	void OnEnable()
	{
		_player.HitTaken += OnHitTaken;
		_player.HitGiven += OnHitGiven;
		_player.SnowballThrown += OnSnowballThrown;
		_player.RefillSnowball += OnRefillSnowball;
		
		player.MaxKillCountChanged += OnMaxKillCountChanged;
	}

	void Start()
	{
		_crownImage.enabled = false;
	}

	void OnDisable()
	{
		_player.HitTaken -= OnHitTaken;
		_player.HitGiven -= OnHitGiven;
		_player.SnowballThrown -= OnSnowballThrown;
		_player.RefillSnowball -= OnRefillSnowball;

		player.MaxKillCountChanged -= OnMaxKillCountChanged;
	}

	void OnHitTaken()
	{
		if (_activeHearts.TryPop(out Image heart))
			heart.color = Color.gray;
	}

	void OnHitGiven()
	{
		if (player.Players.Max(player => player.KillCount) == _player.KillCount)
			SetCrown();
		
		_scoreText.text = $"{_player.KillCount}";
	}

	void OnSnowballThrown()
	{
		_snowballFillImage.fillAmount = 0f;
	}

	void OnRefillSnowball()
	{
		StartCoroutine(RefillSnowball());
	}

	void OnMaxKillCountChanged()
	{
		UnsetCrown();
	}

	void SetCrown()
	{
		_crownImage.enabled = true;
	}

	void UnsetCrown()
	{
		_crownImage.enabled = false;
	}

	IEnumerator RefillSnowball()
	{
		float timer = 0;

		while (timer <= _player.timerReloadMax && _player.canReload == true)
		{
			_snowballFillImage.fillAmount = Mathf.Lerp(0f, 1f, timer / _player.timerReloadMax);

			timer += Time.deltaTime;
			
			yield return null;
		}
		if(_player.canReload == false)
        {
			_snowballFillImage.fillAmount = 0f;
        }

	}
}
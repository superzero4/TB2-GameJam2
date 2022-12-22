using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	[SerializeField] player _player;

	[SerializeField] TextMeshProUGUI _nameText;
	[SerializeField] Image _rightCrownImage;
	[SerializeField] Image _leftCrownImage;
	[SerializeField] List<Image> _heartsImage;
	[SerializeField] TextMeshProUGUI _scoreText;
	[SerializeField] Image _snowballFillImage;

	Stack<Image> _activeHearts;

	Coroutine _refillCoroutine;

	void Awake()
	{
		_activeHearts = new Stack<Image>(_heartsImage);
	}

	void Start()
	{
		UnsetCrowns();

		_player.snowballStatus.OnValueChanged += OnSnowballStatusValueChanged;
		_player.killCount.OnValueChanged += OnKillCountValueChanged;
		_player.health.OnValueChanged += OnHealthValueChanged;
		_player.name.OnValueChanged += OnNameValueChanged;
		_player.hasCrowns.OnValueChanged += OnHasCrownsValueChanged;

		if (!_player.IsOwner)
			return;

		_player.SetNameServerRpc(_player.OwnerClientId);
	}

	void OnDisable()
	{
		_player.name.OnValueChanged -= OnNameValueChanged;
		_player.killCount.OnValueChanged -= OnKillCountValueChanged;
		_player.health.OnValueChanged -= OnHealthValueChanged;
		_player.snowballStatus.OnValueChanged += OnSnowballStatusValueChanged;
		_player.hasCrowns.OnValueChanged -= OnHasCrownsValueChanged;
	}

	void OnKillCountValueChanged(int previousValue, int newValue)
	{
		_scoreText.text = $"{newValue}";
	}

	void OnHealthValueChanged(int previousValue, int newValue)
	{
		if (newValue >= 3)
			return;
		
		if (_activeHearts.TryPop(out Image heart))
			heart.color = Color.gray;
	}

	void OnSnowballStatusValueChanged(SnowballStatus previousValue, SnowballStatus newValue)
	{
		switch (newValue)
		{
			case SnowballStatus.NotReady:
				if (_refillCoroutine is null)
					return;
		
				StopCoroutine(_refillCoroutine);
				_snowballFillImage.fillAmount = 0f;
				break;
			case SnowballStatus.Reloading:
				_refillCoroutine = StartCoroutine(RefillSnowball());
				break;
			case SnowballStatus.Ready:
				_snowballFillImage.fillAmount = 1f;
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
		}
	}

	void OnNameValueChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
	{
		_nameText.text = newValue.Value;
	}

	void OnHasCrownsValueChanged(bool previousValue, bool newValue)
	{
		if (newValue)
			SetCrowns();
		else
			UnsetCrowns();
	}

	void SetCrowns()
	{
		_rightCrownImage.enabled = true;
		_leftCrownImage.enabled = true;
	}

	void UnsetCrowns()
	{
		_rightCrownImage.enabled = false;
		_leftCrownImage.enabled = false;
	}

	IEnumerator RefillSnowball()
	{
		float timer = 0;

		while (timer <= _player.timerReloadMax)
		{
			_snowballFillImage.fillAmount = Mathf.Lerp(0f, 1f, timer / _player.timerReloadMax);

			timer += Time.deltaTime;
			
			yield return null;
		}
		
		_snowballFillImage.fillAmount = 1f;
	}
}
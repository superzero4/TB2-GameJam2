using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
		
	}

    private void OnHitGiven()
    {
		_scoreText.text = $"{_player.KillCount}";
	}

	void Start()
	{
		UnsetCrowns();

		_nameText.text = $"{ServerGameNetPortal.Instance.GetPlayerData(_player.OwnerClientId)?.PlayerName}";
	}

	void OnDisable()
	{
		_player.HitTaken -= OnHitTaken;
		_player.HitGiven -= OnHitGiven;
		_player.SnowballThrown -= OnSnowballThrown;
		_player.RefillSnowball -= OnRefillSnowball;
	}

	void OnHitTaken(ulong x)
	{
		if (_activeHearts.TryPop(out Image heart))
			heart.color = Color.gray;
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
		UnsetCrowns();
	}

	public void SetCrowns()
	{
		_rightCrownImage.enabled = true;
		_leftCrownImage.enabled = true;
	}

	public void UnsetCrowns()
	{
		_rightCrownImage.enabled = false;
		_leftCrownImage.enabled = false;
	}

	IEnumerator RefillSnowball()
	{
		float timer = 0;

		while (timer <= _player.timerReloadMax && _player.canReload)
		{
			_snowballFillImage.fillAmount = Mathf.Lerp(0f, 1f, timer / _player.timerReloadMax);

			timer += Time.deltaTime;
			
			yield return null;
		}
		
		_snowballFillImage.fillAmount = _player.canReload ? 1f : 0f;
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class player : NetworkBehaviour
{
    //Body
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private CapsuleCollider2D _cc;
    [SerializeField] private SpriteRenderer _sr;
    public PlayerManager manager;

    //Move
    public InputActionAsset controls;
    [SerializeField] private float speed = 2;
    private Vector2 newPos;

    //Shoot
    public float angle;
    [SerializeField] private boule b;
    private Vector2 aimDirection;
    private Vector2 mousePosition;
    private new Camera camera;

    //Reload
    private bool canShoot;

    /// <summary>
    /// Internal variable, not synced with animator
    /// </summary>
    private bool unsyncReload = false;

    /// <summary>
    /// Syncs with animator on set
    /// </summary>
    private bool reload
    {
        get => unsyncReload;
        set
        {
            unsyncReload = value;
            animator.ReloadAnimation(value);
        }
    }

    public bool canReload;
    public float timerReloadMax;
    private float timerReload;

    //Animation
    public AnimatorFacade animator;
    [SerializeField] private float TimerClignoteMax;
    private float TimerClignote;
    private bool playerColor;

    public NetworkVariable<bool> collide = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    [SerializeField] private float TimerCollisionMax;
    private float TimerCollision;

    //PauseMenu
    [SerializeField] private GameObject buttonPanel;

    //Others
    [ClientRpc]
    internal void SkinSelectionClientRpc(ulong clientID, int skinIndex)
    {
        if (OwnerClientId != clientID) return;
        animator.PickAnimator(skinIndex);
    }

    public bool canMove = true;

    //Actions
	public Action HitTaken { get; set; }
	public Action HitGiven { get; set; }
	public Action RefillSnowball { get; set; }
	public Action SnowballThrown { get; set; }
	public static Action MaxKillCountChanged { get; set; }

	public Action<player> Died { get; set; }

	public NetworkVariable<bool> hasCrowns = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<FixedString64Bytes> name = new NetworkVariable<FixedString64Bytes>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<SnowballStatus> snowballStatus = new NetworkVariable<SnowballStatus>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<int> killCount = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
	public NetworkVariable<int> health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	[SerializeField] private int initialHealth = 3;
	[SerializeField] private float timeToDied = 1f;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        var cam = FindObjectOfType<CinemachineVirtualCamera>();
        cam.LookAt =  cam.Follow = transform;

        _rb = GetComponent<Rigidbody2D>();
        _cc = GetComponent<CapsuleCollider2D>();

        health.Value = initialHealth;

        //Shoot
        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            if (canShoot)
            {
                Vector2 dir = new Vector2(GetMousePosition().x, GetMousePosition().y);
                ShootServerRpc(dir, angle, OwnerClientId);
                canShoot = false;
            }
        };
        controls.FindActionMap("Player").FindAction("Reload").performed += ctx => { Reload(); };
        controls.Enable();

        //Pause
        buttonPanel.SetActive(false);
        controls.FindActionMap("Player").FindAction("Pause").performed += ctx =>
        {
            Pause();
            Debug.Log("pause");
        };

        //Reload
        canShoot = false;
        reload = false;
        timerReload = timerReloadMax;

        //Animation
        TimerClignote = TimerClignoteMax;
        playerColor = false;
        collide.Value = false;
        TimerCollision = TimerCollisionMax;

        camera = Camera.main;
        animator.PickAnimator(LobbyPlayerStatesContainer._playersData[(int)OwnerClientId].SkinIndex);
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (canMove)
        {
            //Shoot
            mousePosition = GetMousePosition();

            //Move
            Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
            newPos.x = inputVector.x * speed * Time.fixedDeltaTime;
            newPos.y = inputVector.y * speed * Time.fixedDeltaTime;

            //Animation
            animator.SetOrientation(inputVector.x, inputVector.y);

            //Reload
            if (inputVector != Vector2.zero)
			{
				if (snowballStatus.Value == SnowballStatus.Reloading && IsOwner)
					snowballStatus.Value = SnowballStatus.NotReady;
				canReload = false;
                reload = false;
                timerReload = timerReloadMax;
                animator.ReloadAnimation(false);
            }
            else
            {
                canReload = true;
            }

            if (reload && canReload)
            {
                animator.ReloadAnimation(1 - (timerReload / timerReloadMax));
                timerReload -= Time.deltaTime;
                if (timerReload <= 0)
                {
                    canShoot = true;
					if (IsOwner)
						snowballStatus.Value = SnowballStatus.Ready;
                    timerReload = timerReloadMax;
                    reload = false;
                    animator.ReloadAnimation(false);
                }
            }
        }

        //Clignote en rouge
        if (collide.Value)
        {
            TimerCollision -= Time.deltaTime;

            FlickeringServerRpc();

            //Periode d'invincibilit??? ap???rs une collision
            if (TimerCollision <= 0)
            {
                collide.Value = false;

                TimerCollision = TimerCollisionMax;
                TimerClignote = TimerClignoteMax;

                ResetColorMaterialServerRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FlickeringServerRpc()
    {
        if (IsHost)
        {
            TimerClignote -= Time.deltaTime;
            if (TimerClignote <= 0)
            {
                playerColor = !playerColor;
                TimerClignote = TimerClignoteMax;
            }

            _sr.material.color = playerColor ? Color.white : Color.red;
        }

        FlickeringClientRpc();
    }

    [ClientRpc]
    private void FlickeringClientRpc()
    {
        TimerClignote -= Time.deltaTime;
        if (TimerClignote <= 0)
        {
            playerColor = !playerColor;
            TimerClignote = TimerClignoteMax;
        }

        _sr.material.color = playerColor ? Color.white : Color.red;
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetColorMaterialServerRpc()
    {
        if (IsHost)
        {
            _sr.material.color = Color.white;
        }

        ResetColorMaterialClientRpc();
    }

    [ClientRpc]
    private void ResetColorMaterialClientRpc()
    {
        _sr.material.color = Color.white;
    }
    
    public void FixedUpdate()
    {
        //Move
        _rb.MovePosition(_rb.position + newPos);

        //Shoot
        aimDirection = (mousePosition - _rb.position).normalized;
        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
    }

    public void Pause()
    {
        if (buttonPanel.activeSelf)
        {
            buttonPanel.SetActive(false);
        }
        else
        {
            buttonPanel.SetActive(true);
        }
    }

    public void Reload()
	{
		if (IsOwner)
			snowballStatus.Value = SnowballStatus.Reloading;
		
        reload = true;
    }
	
    [ContextMenu("Shoot")]
    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(Vector2 shootDirection, float angle, ulong playerId)
    {
        Vector3 newBoulePos = new Vector3(_rb.position.x + 0.6f * _cc.size.x * Mathf.Cos(angle),
            _rb.position.y + 0.6f * _cc.size.y * Mathf.Sin(angle));
        boule newBoul = Instantiate(b, newBoulePos, Quaternion.identity);
        newBoul.Init(angle, playerId);
        Debug.Log("Lauchernew" + newBoul.launcherId);
        newBoul.GetComponent<NetworkObject>().Spawn();
        animator.ShootToward(shootDirection.x, shootDirection.y);
        ShootClientRpc();
    }

	[ClientRpc]
	public void ShootClientRpc()
	{
		if (!IsOwner)
			return;
		
		snowballStatus.Value = SnowballStatus.NotReady;
	}

    public Vector2 GetMousePosition()
    {
        if (camera == null)
        {
            return Vector2.zero;
        }
        else if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 4000, 0b1 << 6))
        {
            return hitInfo.point;
        }
        else
        {
            return Vector2.zero;
        }
    }

	public void TakeDamage(ulong clientId)
	{
        TakeDamageClientRpc(clientId);
    }

    private ulong clientIdTouched;

    [ClientRpc]
    private void TakeDamageClientRpc(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        clientIdTouched = clientId;
        health.Value--;

		switch (health.Value)
        {
            case 0:
                if (IsHost)
                    PlayerDiedServerRpc();
                else
                    PlayerDied();
                break;
            case > 0 when collide.Value == false:
                collide.Value = true;
                break;
        }
    }

    private void PlayerDied()
    {
        animator.Kill();
        PlayerDiedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerDiedServerRpc()
    {
        if (IsHost)
            animator.Kill();

        StartCoroutine(DiedCoroutine());
        //Destroy(gameObject , timeToDied);
    }

    private IEnumerator DiedCoroutine()
    {
        yield return new WaitForSeconds(timeToDied);
        Died?.Invoke(this);
    }

	[ClientRpc]
	public void InflictDamageClientRpc()
	{
		if (!IsOwner)
			return;

		UpdateLobbyStateKillCountServerRpc((int) OwnerClientId);
		killCount.Value++;
	}

	[ServerRpc]
	public void UpdateLobbyStateKillCountServerRpc(int id)
	{
		LobbyPlayerState lobbyPlayerState = LobbyPlayerStatesContainer._playersData[id];
		lobbyPlayerState.KillCount++;
		LobbyPlayerStatesContainer._playersData[id] = lobbyPlayerState;
	}

	[ServerRpc]
	public void SetNameServerRpc(ulong playerOwnerClientId)
	{
		SetNameClientRpc(ServerGameNetPortal.Instance.GetPlayerData(playerOwnerClientId)?.PlayerName);
	}
	
	[ClientRpc]
	void SetNameClientRpc(FixedString64Bytes _name)
	{
		if (!IsOwner)
			return;
		
		name.Value = _name;
	}

	[ClientRpc]
	public void SetCrownsClientRpc(bool newValue)
	{
		if (!IsOwner)
			return;

		hasCrowns.Value = newValue;
	}
}

public enum SnowballStatus
{
	NotReady,
	Reloading,
	Ready
}
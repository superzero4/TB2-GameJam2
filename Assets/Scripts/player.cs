using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    private bool reload;
    public bool canReload;
    public float timerReloadMax;
    private float timerReload;

    //Animation
    public AnimatorFacade animator;
    [SerializeField] private float TimerClignoteMax;
    private float TimerClignote;
    private bool playerColor;
    public NetworkVariable<bool> collide = new NetworkVariable<bool>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField] private float TimerCollisionMax;
    private float TimerCollision;

    public bool canMove = true;

	public int KillCount { get; set; }
    
    [SerializeField] private int initialHealth = 3;
    private NetworkVariable<int> _health = new NetworkVariable<int>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

	public Action HitTaken { get; set; } 
	public Action HitGiven { get; set; } 
	public Action RefillSnowball { get; set; } 
	public Action SnowballThrown { get; set; }
	public Action<player> Died { get; set; }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        _rb = GetComponent<Rigidbody2D>();
        _cc = GetComponent<CapsuleCollider2D>();

        _health.Value = initialHealth;
        
        //Shoot
        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            if (canShoot)
            {
                Vector2 dir = new Vector2(GetMousePosition().x, GetMousePosition().y);
                ShootServerRpc(dir, angle);
                canShoot = false;
            }  
        };
        controls.FindActionMap("Player").FindAction("Reload").performed += ctx =>
        {
            Reload();
        };
        controls.Enable();

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
                if(timerReload <= 0)
                {   
                    canShoot = true;
                    timerReload = timerReloadMax;
                    reload = false;
                }
            }
        }

        //Clignote en rouge
        if (collide.Value)
        {
            TimerCollision -= Time.deltaTime;
            TimerClignote -= Time.deltaTime;
            if (TimerClignote <= 0)
            {
                playerColor = !playerColor;
                TimerClignote = TimerClignoteMax;
            }
            if (!playerColor)
            {
                _sr.material.color = Color.red;
            }
            if (playerColor)
            {
                _sr.material.color = Color.white;
            }

            //Periode d'invincibilit� ap�rs une collision
            if (TimerCollision <= 0)
            {
                TimerCollision = TimerCollisionMax;
                _sr.material.color = Color.white;
                TimerClignote = TimerClignoteMax;
                collide.Value = false;
            }
        }  
    }

    public void FixedUpdate()
    {
        //Move
        _rb.MovePosition(_rb.position + newPos);

        //Shoot
        aimDirection = (mousePosition - _rb.position).normalized;
        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
    }

	[ContextMenu("Shoot")]
    [ServerRpc(RequireOwnership = false)]
	public void ShootServerRpc(Vector2 shootDirection, float angle)
    {
        Debug.Log(shootDirection);

        Vector3 newBoulePos = new Vector3(_rb.position.x + 0.6f * _cc.size.x * Mathf.Cos(angle), _rb.position.y + 0.6f * _cc.size.y * Mathf.Sin(angle));
        boule newBoul = Instantiate(b , newBoulePos , Quaternion.identity);
        newBoul.GetComponent<NetworkObject>().Spawn();
        newBoul.angle = angle;
        newBoul.launcher = this;
        animator.ShootToward(shootDirection.x, shootDirection.y);
    
        SnowballThrown?.Invoke();
    }

    public void Reload()
    {
        RefillSnowball?.Invoke();
        reload = true;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner) return;
        
        if(_health.Value == 0)
        {
            animator.Kill();
			Died?.Invoke(this);
            Destroy(gameObject , 1);
        }
        if(_health.Value > 0 && collide.Value == false)
        {
            collide.Value = true;
        }
    }

	[ContextMenu("TakeDamage")]
	public void TakeDamage(ulong clientId)
	{
        TakeDamageClientRpc(clientId);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        _health.Value--;
        HitTaken?.Invoke();
    }

	[ContextMenu("InflictDamage")]
	public void InflictDamage()
	{		
        //TODO use Network Manager
		/*KillCount++;
		
		if (KillCount > maxKillCount)
			MaxKillCountChanged?.Invoke();
		
		HitGiven?.Invoke();*/
	}
}
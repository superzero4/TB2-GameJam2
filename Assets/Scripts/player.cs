using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class player : NetworkBehaviour
{
    //Body
    public Rigidbody2D _rb;
    public CapsuleCollider2D _cc;
    [SerializeField]
    private SpriteRenderer _sr;

    //Move
    public InputActionAsset controls;
    [SerializeField]
    private float speed = 2;
    private Vector2 newPos;

    //Shoot
    public float angle;
    [SerializeField]
    private boule b;
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
    [SerializeField]
    private float TimerClignoteMax;
    private float TimerClignote;
    private bool playerColor;
    public bool Collide;
    [SerializeField]
    private float TimerCollisionMax;
    private float TimerCollision;

    public bool canMove = true;

	public int KillCount { get; set; }
	public int Health { get; set; }

	public Action HitTaken { get; set; } 
	public Action HitGiven { get; set; } 
	public Action RefillSnowball { get; set; } 
	public Action SnowballThrown { get; set; }
	
	public static Action MaxKillCountChanged { get; set; }

    private void Awake()
    {
        if (!IsOwner)
            return;

        //Shoot
        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            Shoot();
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
        Collide = false;

        TimerCollision = TimerCollisionMax;
    }

	void OnStartClient()
	{
        if (!IsOwner)
            return;

        camera = Camera.main;
        Health = 3;
	}

    private void Update()
    {
        if (!IsOwner)
           return;

        // Todo : replace by a functional input system
        var moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.Z)) moveDir.y += 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1f;
        if (Input.GetKey(KeyCode.Q)) moveDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1f;

        transform.position += moveDir * 2 * Time.deltaTime;
        
        if (canMove)
        {
	        //Shoot
	        mousePosition = GetMousePosition();

	        //Move
	        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
	        Debug.Log(inputVector);
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
        if (Collide)
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
                Collide = false;
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
	public void Shoot()
    {
        Vector3 newBoulePos = new Vector3(_rb.position.x + 0.6f * _cc.size.x * Mathf.Cos(angle), _rb.position.y + 0.6f * _cc.size.y * Mathf.Sin(angle));
        boule newBoul = Instantiate(b , newBoulePos , Quaternion.identity);
        newBoul.angle = angle;
        newBoul.launcher = this;
        canShoot = false;
        animator.ShootToward(GetMousePosition().x, GetMousePosition().y);
    
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
            return Vector2.zero;

        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 4000, 0b1 << 6))
            return hitInfo.point;
        else
            return Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(Health == 0)
        {
            animator.Kill();
            Destroy(gameObject , 1);
        }
        if(Health > 0 && Collide == false)
        {
            Collide = true;
        }
    }

	[ContextMenu("TakeDamage")]
	public void TakeDamage()
	{
        Health--;
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

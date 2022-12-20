using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class player : MonoBehaviour
{
    //Body
    public Rigidbody2D _rb;
    public CapsuleCollider2D _cc;
    [SerializeField]
    private SpriteRenderer _sr;

    //Move
    public InputActionAsset controls;
    [SerializeField]
    private float speed;
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

    public bool canMove;

	public int KillCount { get; set; }
	public int Health { get; set; }

	public Action HitTaken { get; set; } 
	public Action HitGiven { get; set; } 
	public Action RefillSnowball { get; set; } 
	public Action SnowballThrown { get; set; }
	
	public static Action MaxKillCountChanged { get; set; }

	public static List<player> Players { get; } = new List<player>();

    private void Awake()
    {
        if (canMove)
        {
            //Shoot
            camera = Camera.main;
            controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
            {
                if(canShoot == true)
                {
                    Shoot();
                }     
            };
            controls.FindActionMap("Player").FindAction("Reload").performed += ctx =>
            {
                Reload();
            };
            controls.Enable();      
        }

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

	void OnEnable()
	{
		Players.Add(this);
	}

	void Start()
	{
		Health = 3;
	}

	public void Update()
    {
        if (canMove)
        {
            //Shoot
            mousePosition = GetMousePosition();

            //Move
            Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
            newPos.x = inputVector.x * speed * Time.deltaTime;
            newPos.y = inputVector.y * speed * Time.deltaTime;

            //Animation
            animator.SetOrientation(inputVector.x, inputVector.y);

            //Reload
            if (inputVector != Vector2.zero)
            {
                canReload = false;
                reload = false;
                timerReload = timerReloadMax;
            }
            else
            {
                canReload = true;
            }
            if (reload && canReload)
            {
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

            //Periode d'invincibilité apèrs une collision
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

	void OnDisable()
	{
		Players.Remove(this);
	}

	[ContextMenu("Shoot")]
	public void Shoot()
    {
        boule newBoul = Instantiate(b);
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
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 4000, 0b1 << 6))
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
		int maxKillCount = Players.Max(player => player.KillCount);
		
		KillCount++;
		
		if (KillCount > maxKillCount)
			MaxKillCountChanged?.Invoke();
		
		HitGiven?.Invoke();
	}
}

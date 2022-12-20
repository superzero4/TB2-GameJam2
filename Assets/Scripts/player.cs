using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public Rigidbody2D _rb;
    public CapsuleCollider2D _cc;
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

    //Animation
    public AnimatorFacade animator;


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
        //Shoot
        camera = Camera.main;
        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            Shoot();
        };
        controls.Enable();
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
        //Shoot
        mousePosition = GetMousePosition();

        //Move
        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
        newPos.x = inputVector.x * speed * Time.deltaTime;
        newPos.y = inputVector.y * speed * Time.deltaTime;

        //Animation
        animator.SetOrientation(inputVector.x, inputVector.y);
    }

    public void FixedUpdate()
    {
        //Move
        _rb.MovePosition(_rb.position + newPos);

        //Shoot
        aimDirection = (mousePosition - _rb.position).normalized;
        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
        transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
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
       
        animator.ShootToward(GetMousePosition().x, GetMousePosition().y);
    
		SnowballThrown?.Invoke();
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

	[ContextMenu("RefillSnowball")]
	public void ReefillSnowball()
	{
		RefillSnowball?.Invoke();
	}
}
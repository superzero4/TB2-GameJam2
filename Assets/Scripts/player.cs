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
    public Rigidbody2D _rb;
    public CapsuleCollider2D _cc;

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
    public float timerReloadMax;
    private float timerReload;
    [SerializeField]
    private Slider _slider;

    //Animation
    public AnimatorFacade animator;

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
        canShoot = true;
        reload = false;
        timerReload = timerReloadMax;
        _slider.gameObject.SetActive(false);
        _slider.maxValue = timerReloadMax;
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
	        _slider.gameObject.SetActive(reload);
	        if (reload)
	        {
		        timerReload -= Time.fixedDeltaTime;
		        _slider.value = timerReloadMax - timerReload;
		        if (timerReload <= 0)
		        {
			        canShoot = true;
			        timerReload = timerReloadMax;
			        reload = false;
		        }
	        }
        }
        
        //Shoot
        aimDirection = (mousePosition - _rb.position).normalized;
        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
    }

    public void FixedUpdate()
    {
        if (!IsOwner)
            return;
        
        //Move
        _rb.MovePosition(_rb.position + newPos);
    }

	[ContextMenu("Shoot")]
	public void Shoot()
    {
        if (canShoot == false)
            return;

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
        if (camera == null)
            return Vector2.zero;

        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 4000, 0b1 << 6))
            return hitInfo.point;
        else
            return Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        animator.Kill();
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

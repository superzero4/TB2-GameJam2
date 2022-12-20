using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public InputActionAsset controls;

    private Vector3 mousePosition;
    private Vector3 aimDirection;
    private Vector3 newPos;

    private new Camera camera;

    //In Radian
    public float angle;

    [SerializeField]
    private GameObject boule;
    [SerializeField]
    private float speed;

	public int KillCount { get; set; }
	public int Health { get; set; }

	public Action HitTaken { get; set; } 
	public Action HitGiven { get; set; } 
	public Action RefillSnowball { get; set; } 
	public Action SnowballThrown { get; set; }
	
	public static Action MaxKillCountChanged { get; set; }

	public static List<Player> Players { get; } = new List<Player>();

    private void Awake()
    {
        newPos = transform.position;

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
        mousePosition = GetMousePosition();
        aimDirection = (mousePosition - transform.position).normalized;


        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
        newPos.x += inputVector.x * speed * Time.deltaTime;
        newPos.y += inputVector.y * speed * Time.deltaTime;

        transform.position = newPos;

        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);
        transform.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
    }

	void OnDisable()
	{
		Players.Remove(this);
	}

	public void Shoot()
    {
        GameObject newBoul = Instantiate<GameObject>(boule);
        newBoul.GetComponent<Boule>().angle = angle;
        Vector3 newBoulpos = Vector3.zero;
        newBoulpos.x = transform.position.x;
        newBoulpos.y = transform.position.y;
        newBoulpos.z = transform.position.z;
        newBoul.transform.position = newBoulpos;
		
		SnowballThrown?.Invoke();
    }

    public Vector3 GetMousePosition()
    {
        if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo, 4000, 0b1 << 6))
        {
            return hitInfo.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

	public void TakeDamage()
	{
		Health--;
		HitTaken?.Invoke();
	}

	public void InflictDamage()
	{
		int maxKillCount = Players.Max(player => player.KillCount);
		
		KillCount++;
		
		if (KillCount > maxKillCount)
			MaxKillCountChanged?.Invoke();
		
		HitGiven?.Invoke();
	}
}
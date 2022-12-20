using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    //Reload
    private bool canShoot;
    private bool reload;
    [SerializeField]
    private float timerReloadMax;
    private float timerReload;
    [SerializeField]
    private Slider _slider;

    //Animation
    public AnimatorFacade animator;

    public bool canMove;

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
        canShoot = true;
        reload = false;
        timerReload = timerReloadMax;
        _slider.gameObject.SetActive(false);
        _slider.maxValue = timerReloadMax;
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
            _slider.gameObject.SetActive(reload);
            if (reload)
            {
                timerReload -= Time.deltaTime;
                _slider.value = timerReloadMax - timerReload;
                if(timerReload <= 0)
                {   
                    canShoot = true;
                    timerReload = timerReloadMax;
                    reload = false;
                }
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

    public void Shoot()
    {
        boule newBoul = Instantiate(b);
        newBoul.angle = angle;
        newBoul.launcher = this;
        canShoot = false;
        animator.ShootToward(GetMousePosition().x, GetMousePosition().y);
    }

    public void Reload()
    {
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
        animator.Kill();
    }
}

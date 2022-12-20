using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    //Move
    public InputActionAsset controls;
    [SerializeField]
    private float speed;
    private Vector3 newPos;

    //Shoot
    public float angle;
    [SerializeField]
    private GameObject boule;
    private Vector3 aimDirection;
    private Vector3 mousePosition;
    private new Camera camera;

    //Animation
    public AnimatorFacade animator;


    private void Awake()
    {
        //Move
        newPos = transform.position;

        //Shoot
        camera = Camera.main;
        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            Shoot();
        };
        controls.Enable();
    }

    public void Update()
    {
        //Shoot
        mousePosition = GetMousePosition();
        aimDirection = (mousePosition - transform.position).normalized;
        angle = Mathf.Atan2(aimDirection.y, aimDirection.x);

        //Move
        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
        newPos.x += inputVector.x * speed * Time.deltaTime;
        newPos.y += inputVector.y * speed * Time.deltaTime;
        transform.position = newPos;

        //Animation
        animator.SetOrientation(inputVector.x, inputVector.y);
    }
    public void Shoot()
    {
        GameObject newBoul = Instantiate<GameObject>(boule);
        newBoul.GetComponent<boule>().angle = angle;
        Vector3 newBoulpos = Vector3.zero;
        newBoulpos.x = transform.position.x;
        newBoulpos.y = transform.position.y;
        newBoulpos.z = transform.position.z;
        newBoul.transform.position = newBoulpos;
        animator.ShootToward(GetMousePosition().x, GetMousePosition().y);
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
}

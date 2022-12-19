using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
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
    public void Shoot()
    {
        GameObject newBoul = Instantiate<GameObject>(boule);
        newBoul.GetComponent<boule>().angle = angle;
        Vector3 newBoulpos = Vector3.zero;
        newBoulpos.x = transform.position.x;
        newBoulpos.y = transform.position.y;
        newBoulpos.z = transform.position.z;
        newBoul.transform.position = newBoulpos;
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

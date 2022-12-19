using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public InputActionAsset controls;

    private Vector3 mousePosition;
    private Vector3 aimDirection;
    private Vector3 newPos;
    public float angle;

    [SerializeField]
    private GameObject boule;
    [SerializeField]
    private float speed;

    private void Awake()
    {
        newPos = transform.position;

        controls.FindActionMap("Player").FindAction("Shoot").performed += ctx =>
        {
            Shoot();
        };

        controls.Enable();
    }

    public void Update()
    {
        mousePosition = Input.mousePosition;
        aimDirection = (mousePosition - transform.position).normalized;


        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
        newPos.x += inputVector.x * speed * Time.deltaTime;
        newPos.y += inputVector.y * speed * Time.deltaTime;

        transform.position = newPos;

        angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);
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

}

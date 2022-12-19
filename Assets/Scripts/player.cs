using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    public InputActionAsset controls;
    public Vector3 mousePosition { get; private set; }

    private Vector3 newPos;

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

        Vector2 inputVector = controls.FindActionMap("Player").FindAction("Movement").ReadValue<Vector2>();
        newPos.x += inputVector.x * speed * Time.deltaTime;
        newPos.y += inputVector.y * speed * Time.deltaTime;

        transform.position = newPos;
    }
    public void Shoot()
    {
        Instantiate(boule , transform.position , Quaternion.identity);
        //Debug.Log(Mathf.Atan((mousePosition.y - transform.position.y) / (mousePosition.x - transform.position.x)) * Mathf.Rad2Deg);
        Debug.Log(Mathf.PI  * Mathf.Rad2Deg);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boule : MonoBehaviour
{
    public float angle;

    private Vector3 newPos;

    private float shootSpeed;

    void Start()
    {
        newPos = transform.position;
        shootSpeed = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        newPos.x += shootSpeed * Time.deltaTime * Mathf.Cos(angle);
        newPos.y += shootSpeed * Time.deltaTime * Mathf.Sin(angle);

        transform.position = newPos;
    }
}

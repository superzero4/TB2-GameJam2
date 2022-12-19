using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boule : MonoBehaviour
{
    public Vector3 mousePosition { get; private set; }

    private Vector3 newPos;

    private float angle;

    void Start()
    {
        mousePosition = Input.mousePosition;
        newPos = transform.position;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

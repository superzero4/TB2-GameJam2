using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boule : MonoBehaviour
{
    public float angle;

    private Vector3 newPos;

    private float shootSpeed;

    void Start()
    {
        newPos = transform.position;
        shootSpeed = 5f;
    }

    void Update()
    {
        newPos.x += shootSpeed * Time.deltaTime * Mathf.Cos(angle);
        newPos.y += shootSpeed * Time.deltaTime * Mathf.Sin(angle);

        transform.position = newPos;
    }

	void OnCollisionEnter2D(Collision2D col)
	{
		if (col.collider.TryGetComponent(out Player player))
		{
			player.TakeDamage();
			// launcher.InflictDamage();
		}
	}
}
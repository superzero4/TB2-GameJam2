using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boule : MonoBehaviour
{
    public float angle;
    [SerializeField]
    private float shootSpeed;
    [SerializeField]
    private Rigidbody2D _rb;
    public player launcher;

    void Start()
    {
        
        //Position d�part
        Vector2 newPos = new Vector2(launcher._rb.position.x + 0.6f * launcher._cc.size.x * Mathf.Cos(angle), launcher._rb.position.y + 0.6f * launcher._cc.size.y * Mathf.Sin(angle));
        _rb.MovePosition(newPos);
        
        //Shoot
        Vector2 force = new Vector2(shootSpeed * Mathf.Cos(angle), shootSpeed * Mathf.Sin(angle));
        _rb.AddForce(force , ForceMode2D.Impulse);
        Debug.Log(force);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

	void OnCollisionEnter2D(Collision2D col)
	{
		if (!col.collider.TryGetComponent(out player player))
			return;
		
		player.TakeDamage();
		launcher.InflictDamage();
	}
}
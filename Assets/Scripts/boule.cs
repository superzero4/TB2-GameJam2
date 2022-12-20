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
    [SerializeField]
    private ParticleSystem _ps;

    void Start()
    {
        //Shoot
        Vector2 force = new Vector2(shootSpeed * Mathf.Cos(angle), shootSpeed * Mathf.Sin(angle));
        _rb.AddForce(force , ForceMode2D.Impulse);

        //Position d�part
        Vector2 newPos = new Vector2(launcher._rb.position.x + 0.6f * launcher._cc.size.x * Mathf.Cos(angle), launcher._rb.position.y + 0.6f * launcher._cc.size.y * Mathf.Sin(angle));
        _rb.MovePosition(newPos);
        transform.right = -force;

        //Particles
        _ps.Play();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {    
        if (collision.TryGetComponent(out player player))
        {
            if(player == launcher)
            {
                return;
            }
            player.TakeDamage();
            launcher.InflictDamage();
        }

        //Particles
        _ps.Play();

        Destroy(gameObject);
    }
}
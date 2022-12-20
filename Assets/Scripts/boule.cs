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
            if(player.Collide == false)
            {
                player.TakeDamage();
                launcher.InflictDamage();
            }  
        }

        //Particles
        _ps.Play();
        Destroy(gameObject);
    }
}
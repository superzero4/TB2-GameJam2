using System;
using Unity.Netcode;
using UnityEngine;

public class boule : NetworkBehaviour
{
    public float angle;
    [SerializeField]
    private float shootSpeed;
    [SerializeField]
    private Rigidbody2D _rb;
    public player launcher;
    [SerializeField]
    private ParticleSystem _ps;
    [SerializeField]
    private Collider2D _collider;
    [SerializeField]
    private Renderer _renderer;

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
        if (!IsServer) return;
        
        if (collision.TryGetComponent(out player player))
        {
            if(player == launcher)
            {
                return;
            }
            if(player.collide.Value == false)
            {
                player.TakeDamage(player.OwnerClientId);
                launcher.InflictDamage();
            }  
        }

        //Particles
        _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _ps.Play();
        _renderer.enabled = false;
        _collider.enabled = false;
        _rb.velocity = Vector2.zero;
        DestroyServerRpc();
    }

    [ServerRpc]
    private void DestroyServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject, 2);
    }
}
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class boule : NetworkBehaviour
{
    public float angle;
    [SerializeField]
    private float shootSpeed;
    [SerializeField]
    private Rigidbody2D _rb;
    public NetworkVariable<ulong> launcherId2 = new NetworkVariable<ulong>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public ulong launcherId => launcherId2.Value;
    public player launcher => PlayerManager.GetPlayer(launcherId);
    [SerializeField]
    private ParticleSystem _ps;
    [SerializeField]
    private Collider2D _collider;
    [SerializeField]
    private Renderer _renderer;
    private AudioManager audioManager;
    private float timeToDestroy = 2f;

    private ulong _tempPlayerId;

    public void Init(float tempAngle, ulong playerId)
    {
        angle = tempAngle;
        launcherId2 = new NetworkVariable<ulong>(playerId);
    }

    public override void OnNetworkSpawn()
    {
        //Shoot
        Vector2 force = new Vector2(shootSpeed * Mathf.Cos(angle), shootSpeed * Mathf.Sin(angle));
        _rb.AddForce(force , ForceMode2D.Impulse);

        //Position d�part
        transform.right = -force;

        //Particles
        _ps.Play();

        //Audio
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        
        if (collision.TryGetComponent(out player player))
        {
            if(player.OwnerClientId == launcherId)
            {
                return;
            }
            if(player.collide.Value == false)
            {
                launcher.HitGiven?.Invoke();
                player.TakeDamage(player.OwnerClientId);
                launcher.InflictDamage();
                audioManager.Play("Aie");
            }  
        }

        //Particles
        
        _rb.velocity = Vector2.zero;
        Debug.Log("flex" + _renderer.enabled);
        DestroyServerRpc();
    }

    [ServerRpc]
    private void DestroyServerRpc()
    {
        HideClientRpc();
        StartCoroutine(DestroyCoroutine());
        audioManager.Play("Boule");
    }

    [ClientRpc]
    private void HideClientRpc()
    {
        _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _ps.Play();
        _renderer.enabled = false;
        _collider.enabled = false;
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(timeToDestroy);
        GetComponent<NetworkObject>().Despawn();
    }
}
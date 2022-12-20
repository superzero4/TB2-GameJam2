using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoneFloat : MonoBehaviour
{
    public float speed;
#pragma warning disable CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant
    private Light light;
#pragma warning restore CS0108 // Un membre masque un membre hérité ; le mot clé new est manquant

    void Start()
    {
        light = GetComponentInChildren<Light>();
    }

    void Update()
    {
        light.intensity = 0.6f + Mathf.Sin(Time.time * speed) * 0.3f;
        transform.position = new Vector3(transform.position.x, -1 + Mathf.Sin(Time.time * speed) * 0.3f, transform.position.z);
    }
}

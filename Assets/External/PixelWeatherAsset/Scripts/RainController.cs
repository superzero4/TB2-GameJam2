﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RainController : MonoBehaviour
{
    [Range(0, 1f)]
    public float masterIntensity = 1f;
    [Range(0, 1f)]
    public float rainIntensity = 1f;
    [Range(0, 1f)]
    public float windIntensity = 1f;
    [Range(0, 1f)]
    public float fogIntensity = 1f;
    [Range(0, 1f)]
    public float lightningIntensity = 1f;
    public bool autoUpdate;

    public ParticleSystem rainPart;
    public ParticleSystem windPart;
    public ParticleSystem lightningPart;
    public ParticleSystem fogPart;

    private ParticleSystem.EmissionModule rainEmission;
    private ParticleSystem.ForceOverLifetimeModule rainForce;
    private ParticleSystem.EmissionModule windEmission;
    private ParticleSystem.MainModule windMain;
    private ParticleSystem.EmissionModule lightningEmission;
    private ParticleSystem.MainModule lightningMain;
    private ParticleSystem.EmissionModule fogEmission;

    void Awake()
    {
        rainEmission = rainPart.emission;
        rainForce = rainPart.forceOverLifetime;
        windEmission = windPart.emission;
        windMain = windPart.main;
        lightningEmission = lightningPart.emission;
        lightningMain = lightningPart.main;
        fogEmission = fogPart.emission;
        UpdateAll();
    }

    void Update()
    {
        if (autoUpdate)
            UpdateAll();
    }

    void UpdateAll(){
#pragma warning disable CS0618 // Le type ou le membre est obsolète
        rainEmission.rate = 200f * masterIntensity * rainIntensity;
#pragma warning restore CS0618 // Le type ou le membre est obsolète
        rainForce.x = new ParticleSystem.MinMaxCurve(-25f * windIntensity * masterIntensity, (-3-30f * windIntensity) * masterIntensity);
#pragma warning disable CS0618 // Le type ou le membre est obsolète
        windEmission.rate = 5f * masterIntensity * (windIntensity + fogIntensity);
#pragma warning restore CS0618 // Le type ou le membre est obsolète
        windMain.startLifetime = 2f + 5f * (1f - windIntensity);
        windMain.startSpeed = new ParticleSystem.MinMaxCurve(15f * windIntensity, 25 * windIntensity);
#pragma warning disable CS0618 // Le type ou le membre est obsolète
        fogEmission.rate = (1f + (rainIntensity + windIntensity)*0.5f) * fogIntensity * masterIntensity;
#pragma warning restore CS0618 // Le type ou le membre est obsolète
        if (rainIntensity * masterIntensity < 0.7f)
#pragma warning disable CS0618 // Le type ou le membre est obsolète
            lightningEmission.rate = 0;
#pragma warning restore CS0618 // Le type ou le membre est obsolète
        else
#pragma warning disable CS0618 // Le type ou le membre est obsolète
            lightningEmission.rate = lightningIntensity * masterIntensity * 0.4f;
#pragma warning restore CS0618 // Le type ou le membre est obsolète
    }

    public void OnMasterChanged(float value)
    {
        masterIntensity = value;
        UpdateAll();
    }
    public void OnRainChanged(float value)
    {
        rainIntensity = value;
        UpdateAll();
    }
    public void OnWindChanged(float value)
    {
        windIntensity = value;
        UpdateAll();
    }
    public void OnLightningChanged(float value)
    {
        lightningIntensity = value;
        UpdateAll();
    }
    public void OnFogChanged(float value)
    {
        fogIntensity = value;
        UpdateAll();
    }
}

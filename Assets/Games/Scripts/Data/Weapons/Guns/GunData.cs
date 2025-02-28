using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class GunData
{
    public GunType gunType;

    public string gunId;
    public string gunName;
    [FormerlySerializedAs("bulletID")] public string bulletId;
    public Sprite gunImage;
    public int magazineSize;
    public float fireRate;
    public float reloadTime;
    
    public GameObject impactEffect;
    
    public AudioClip shootSFX;
    public AudioClip reloadSFX;
    
}

public enum GunType
{
    Pistol,
    SMG,
    Rifle,
    Grenade_Launcher
}
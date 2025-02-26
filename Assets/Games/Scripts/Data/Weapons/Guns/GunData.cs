using System;
using UnityEngine;

[Serializable]
public class GunData
{
    public GunType gunType;

    public string gunId;
    public string gunName;
    public string bulletID;
    
    public Sprite gunImage;

    public int magazineSize;
    
    public float fireRate;
    public float reloadTime;
    
    public GameObject vfx;
    public GameObject vfxHit;
    
    public AudioClip sfx;
    
}

public enum GunType
{
    Pistol,
    SMG,
    Rifle,
    Grenade_Launcher
}
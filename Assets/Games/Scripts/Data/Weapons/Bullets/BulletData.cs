using System;
using UnityEngine;

[Serializable]
public class BulletData
{
    public BulletType bulletType;
    
    public string bulletId;
    public string bulletName;

    public float bulletDamage;
    public float bulletLifetime;
    public float bulletSpeed;
    public float bulletRadius;
    public float bulletGravity;

    public GameObject hitVFX;
    public GameObject explosionVFX;
}

public enum BulletType
{
    Pistol_Bullet,
    SMG_Bullet,
    Rifle_Bullet,
    Grenade_Bullet
}
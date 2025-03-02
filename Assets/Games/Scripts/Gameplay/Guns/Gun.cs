using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class Gun : MonoBehaviour
{
    [SerializeField] private string _gunName;
    [SerializeField] private int _ammo = 30;
    [SerializeField] private int _magazineMax = 30;
    [SerializeField] private float _fireRate = 0.2f;
    [SerializeField] private float _reloadTime = 0.5f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _gunPosition;
    [SerializeField] private GameObject _impactEffect;

    [SerializeField] private AudioClip _shootSFX;
    [SerializeField] private AudioClip _reloadSFX;

    [SerializeField] private GunType _gunType;
    
    private GunInstaller.Settings _gunConfig;
    private GunData _gunData = new GunData();
    
    private string _gunId;
    private string _bulletId;
    private int _magazineSize = 30;
    private float _nextFireTime = 0f;

    public GunType GunType => _gunType;
    public Transform GetFirePoint() => _firePoint;
    public float GetBulletSpeed() => _bulletData.bulletSpeed;
    public float GetBulletGravity() => _bulletData.bulletGravity;
    public float GetBulletRadius() => _bulletData.bulletRadius;
    
    #region Bullet Entity
    
    private BulletInstaller.Settings _bulletConfig;
    private BulletData _bulletData = new BulletData();
    private Bullet_556.Factory _bullet556Factory;
    private Bullet_40.Factory _bullet40Factory;

    #endregion

    [Inject]
    public void Construct(
        GunInstaller.Settings gunConfig,
        BulletInstaller.Settings bulletConfig,
        Bullet_556.Factory bullet556Factory,
        Bullet_40.Factory bullet40Factory)
    {
        _gunConfig = gunConfig;
        _bulletConfig = bulletConfig;
        _bullet556Factory = bullet556Factory;
        _bullet40Factory = bullet40Factory;
    }
    
    public void Initialize(string gunId)
    {
        _gunId = gunId;
        _gunData = _gunConfig.GetGunDataById(gunId);

        _bulletId = _gunData.bulletId;
        _bulletData = _bulletConfig.GetBulletDataById(_bulletId);
        
        _gunName = _gunData.gunName;
        _ammo = _gunData.magazineSize;
        _magazineMax = _ammo * 3;
        _magazineSize = _gunData.magazineSize;
        _fireRate = _gunData.fireRate;
        _reloadTime = _gunData.reloadTime;
        _impactEffect = _gunData.impactEffect;
        _shootSFX = _gunData.shootSFX;
        _reloadSFX = _gunData.reloadSFX;
        _gunType = _gunData.gunType;
    }
    
    public void Shoot(Vector3 targetPosition)
    {
        Vector3 aimDir = (targetPosition - _firePoint.position).normalized;

        if (_ammo > 0 && Time.time >= _nextFireTime)
        {
            _ammo--;
            _nextFireTime = Time.time + _fireRate;
            
            var impactEffectIstance = Instantiate(_impactEffect, _firePoint.position, _firePoint.rotation) as GameObject;
            Destroy(impactEffectIstance, 4);
            
            Bullet bullet = SpawnBullet(_bulletData, aimDir);
            bullet.Shooting();
        }
    }

    public void Reload()
    {
        int ammoMiss = _magazineSize - _ammo;
        if (_magazineMax >= ammoMiss)
        {
            _magazineMax -= ammoMiss;
            _ammo = _magazineSize;
        }
        else
        {
            _ammo += _magazineMax;
            _magazineMax = 0;
        }
    }

    private Bullet SpawnBullet(BulletData bulletData, Vector3 aimDir)
    {
        Bullet bullet = CreateBullet(_bulletId);
        bullet.Initialize(bulletData);
       bullet.transform.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

        return bullet;
    }

    private Bullet CreateBullet(string bulletId)
    {
        Bullet pro = null;
        switch (bulletId)
        {
            case GameDefine.BulletEntity.BULLET_556:
                pro = _bullet556Factory.Create(_firePoint.position);
                break;
            case GameDefine.BulletEntity.BULLET_40:
                pro = _bullet40Factory.Create(_firePoint.position);
                break;
        }
        
        return pro;
    }
}
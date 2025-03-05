using System.Collections;
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
    private float _gunWeight = 0;

    public GunType GunType => _gunType;

    public int GetCurrentAmmo() => _ammo;
    public int GetAmmoRemaining() => _magazineMax;
    public Transform GetFirePoint() => _firePoint;
    public float GetBulletSpeed() => _bulletData.bulletSpeed;
    public float GetBulletGravity() => _bulletData.bulletGravity;
    public float GetBulletRadius() => _bulletData.bulletRadius;
    public float GetGunWeight() => _gunWeight;
    public string GetGunId() => _gunId;
    public int GetTotalAmmo() => (_ammo + _magazineMax);
    
    private bool _isReloading;
    private Player _player;
    private Zombie _zombie;

    public bool IsReloading
    {
        get => _isReloading;
        set => _isReloading = value;
    }
    
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
        _magazineMax = _ammo * 10;
        _magazineSize = _gunData.magazineSize;
        _fireRate = _gunData.fireRate;
        _reloadTime = _gunData.reloadTime;
        _impactEffect = _gunData.impactEffect;
        _shootSFX = _gunData.shootSFX;
        _reloadSFX = _gunData.reloadSFX;
        _gunType = _gunData.gunType;
        _gunWeight = _gunData.gunWeight;
    }

    public void SetPlayer(Player player)
    {
        _player = player;
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
            
            AudioSource.PlayClipAtPoint(_shootSFX, _firePoint.position, 0.5f);
            bullet.Shooting();

            if (_ammo <= 0 && _magazineMax != 0)
            {  
                Reload();
            }
        }
    }

    public void Reload()
    {
        if (!_isReloading && _magazineMax > 0) // Kiểm tra xem có đang reload không
        {
            AudioSource.PlayClipAtPoint(_reloadSFX, _firePoint.position, 0.5f);

            StartCoroutine(ReloadCoroutine());
        }
    }
    
    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        float timer = 0f;

        while (timer < _reloadTime)
        {
            timer += Time.deltaTime;
            if (_player != null)
            {
                _player.GetUIGameplay().ReloadImage.fillAmount = 1f - (timer / _reloadTime);
            }
            yield return null;
        }

        if (_player != null)
        {
            _player.GetUIGameplay().ReloadImage.fillAmount = 0f;
        }
        // Thực hiện hành động reload
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

        _isReloading = false;
    }

    private Bullet SpawnBullet(BulletData bulletData, Vector3 aimDir)
    {
        Bullet bullet = CreateBullet(_bulletId);
        bullet.Initialize(bulletData, _firePoint);
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
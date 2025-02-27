using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private string _gunName;
    [SerializeField] private int _ammo = 30;
    [SerializeField] private int _magazineSize = 30;
    [SerializeField] private float _fireRate = 0.2f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _gunPosition;
    [SerializeField] private Transform _pfBulletProjectile;

    [SerializeField] private GameObject ImpactEffect;

    private float _nextFireTime = 0f;

    public void Shoot(Vector3 targetPosition)
    {
        Vector3 aimDir = (targetPosition - _firePoint.position).normalized;

        if (_ammo > 0 && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;
            
            var impactEffectIstance = Instantiate(ImpactEffect, _firePoint.position, _firePoint.rotation) as GameObject;
            Destroy(impactEffectIstance, 4);
            
            Instantiate(_pfBulletProjectile, transform.position, Quaternion.LookRotation(aimDir, Vector3.up));
        }
    }
}
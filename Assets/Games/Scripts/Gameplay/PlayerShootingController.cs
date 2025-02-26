using System;
using System.Collections;
using StarterAssets;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerShootingController : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera _aimCamera;
    
    [SerializeField]
    private InputSystem _inputSystem;
    
    [SerializeField] private ThirdPersonController _thirdPersonController;
    
    [SerializeField] LayerMask _shootColliderLayerMask;

    [SerializeField] private float _normalSensitivity;
    [SerializeField] private float _shootSensitivity;
    [SerializeField] private Transform _debugTransform;
    [SerializeField] private Transform _pfBulletProjectile;
    [SerializeField] private Transform _spawnBulletPosition;
    
    [SerializeField] private float _fireRate = 0.5f; // Tốc độ bắn (số giây giữa các lần bắn)
    private float _nextFireTime = 0f; // Thời gian tiếp theo có thể bắn

    private bool _isRotating = false;
    private const float _shootAngleThreshold = 45f; // Góc 45 độ

    private void Awake()
    {
        
    }

    private void Update()
    {
        Vector3 _mouseWorldPosition = Vector3.zero;
        
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, _shootColliderLayerMask))
        {
            _debugTransform.position = hit.point;
            _mouseWorldPosition = hit.point;
        }

        if (_inputSystem.shoot && Time.time >= _nextFireTime)
        {
            Vector3 _worldAimTarget = _mouseWorldPosition;
            _worldAimTarget.y = transform.position.y;
            Vector3 _aimDirection = (_worldAimTarget - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, _aimDirection);

            if (angleToTarget <= _shootAngleThreshold)
            {
                // Nếu góc <= 45 độ, vừa bắn vừa xoay
                _nextFireTime = Time.time + _fireRate;
                InstantiateBullet(_mouseWorldPosition);
                RotateTowardsTarget(_aimDirection);
            }
            else
            {
                // Nếu góc > 45 độ, xoay trước rồi mới bắn
                if (!_isRotating)
                {
                    StartCoroutine(RotateAndShoot(_mouseWorldPosition));
                }
            }
        }
        else if (!_inputSystem.shoot)
        {
            _aimCamera.gameObject.SetActive(false);
            _thirdPersonController.SetSensitivity(_shootSensitivity);
            _thirdPersonController.SetRotateOnMove(true);
        }
    }

    private IEnumerator RotateAndShoot(Vector3 mouseWorldPosition)
    {
        _isRotating = true;
        _aimCamera.gameObject.SetActive(true);
        _thirdPersonController.SetSensitivity(_normalSensitivity);
        _thirdPersonController.SetRotateOnMove(false);

        Vector3 _worldAimTarget = mouseWorldPosition;
        _worldAimTarget.y = transform.position.y;
        Vector3 _aimDirection = (_worldAimTarget - transform.position).normalized;

        while (Vector3.Angle(transform.forward, _aimDirection) > 0.1f)
        {
            RotateTowardsTarget(_aimDirection);
            yield return null;
        }

        _nextFireTime = Time.time + _fireRate;
        InstantiateBullet(mouseWorldPosition);

        _isRotating = false;
    }

    private void RotateTowardsTarget(Vector3 aimDirection)
    {
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }

    private void InstantiateBullet(Vector3 targetPosition)
    {
        Vector3 aimDir = (targetPosition - _spawnBulletPosition.position).normalized;
        Instantiate(_pfBulletProjectile, _spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));
    }

}

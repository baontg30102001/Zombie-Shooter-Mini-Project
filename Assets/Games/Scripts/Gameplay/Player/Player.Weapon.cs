using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

public partial class Player : MonoBehaviour
{
    [FoldoutGroup("Player Setup"), SerializeField]
    private CinemachineCamera _aimCamera;

    [FoldoutGroup("Player Shooting"), SerializeField]
    private LayerMask _shootColliderLayerMask;

    [FoldoutGroup("Player Shooting"), SerializeField]
    private float _normalSensitivity;

    [FoldoutGroup("Player Shooting"), SerializeField]
    private float _shootSensitivity;

    [FoldoutGroup("Player Shooting"), SerializeField]
    private Transform _gunPosition;

    [FoldoutGroup("Player Shooting"), SerializeField]
    private GameObject _aoeIndicatorPrefab;

    private const float _shootAngleThreshold = 45f;
    private GameObject _aoeIndicatorInstance;
    private bool _isAimingGrenadeLauncher;

    private void HandlerWeapon()
    {
        if (_currentGun == null) return;
        if (_currentGun.GunType == GunType.Grenade_Launcher)
        {
            HandleGrenadeLauncher();
        }
        else
        {
            HandleNormalGun();
        }

        if (_inputSystem.reload)
        {
            _currentGun.Reload();
            _inputSystem.reload = false;
        }

        if (_inputSystem.swapWeapon)
        {
            if (_gunInInventory.Count > 1)
            {
                int index = _gunInInventory.IndexOf(_currentGun);
                if (index < _gunInInventory.Count - 1)
                {
                    index++;
                    CurrentGun = _gunInInventory[index];
                }
                else
                {
                    CurrentGun = _gunInInventory[0];
                }
            }

            _inputSystem.swapWeapon = false;
        }
    }

    private void HandleNormalGun()
    {
        if (_inputSystem.shoot)
        {
            Vector3 worldAimTarget = GetTargetPosition();
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            _aimCamera.gameObject.SetActive(true);
            _sensitivity = _shootSensitivity;
            _rotateOnMove = false;

            float angleToTarget = Vector3.Angle(transform.forward, aimDirection);

            if (angleToTarget <= _shootAngleThreshold)
            {
                RotateTowardsTarget(aimDirection);
                _currentGun.Shoot(GetTargetPosition());
            }
            else
            {
                RotateTowardsTarget(aimDirection);
            }
        }
        else
        {
            _aimCamera.gameObject.SetActive(false);
            _sensitivity = _normalSensitivity;
            _rotateOnMove = true;
        }
    }

    private void HandleGrenadeLauncher()
    {
        Vector3 worldAimTarget = GetTargetPosition();
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

        if (_inputSystem.shoot && !_isAimingGrenadeLauncher)
        {
            _isAimingGrenadeLauncher = true;
            _aimCamera.gameObject.SetActive(true);
            _sensitivity = _shootSensitivity;
            _rotateOnMove = false;
            _aoeIndicatorInstance.SetActive(true);
            
            (_currentGun as M32A1).LineRenderer.gameObject.SetActive(true);
        }
        else if (!_inputSystem.shoot && _isAimingGrenadeLauncher)
        {
            _isAimingGrenadeLauncher = false;
            _aimCamera.gameObject.SetActive(false);
            _sensitivity = _normalSensitivity;
            _rotateOnMove = true;
            _currentGun.Shoot(GetTargetPosition());
            _aoeIndicatorInstance.SetActive(false);
            
            (_currentGun as M32A1).LineRenderer.gameObject.SetActive(false);
        }
        else
        {
            DrawProjection();
            RotateTowardsTarget(aimDirection);
        }
    }

    private Vector3 GetTargetPosition()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, _shootColliderLayerMask))
        {
            return hit.point;
        }
        else
        {
            return ray.GetPoint(999f);
        }
    }

    private void RotateTowardsTarget(Vector3 aimDirection)
    {
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }
    
    private void DrawProjection()
    {
        (_currentGun as M32A1).LineRenderer.enabled = true;
        (_currentGun as M32A1).LineRenderer.positionCount = Mathf.CeilToInt(30 / 0.1f) + 1;
        Vector3 startPosition = _currentGun.GetFirePoint().position;
        Vector3 startVelocity = _currentGun.GetBulletSpeed() * _aimCamera.transform.forward;
        int i = 0;
        (_currentGun as M32A1).LineRenderer.SetPosition(i, startPosition);
        for (float time = 0; time < 30; time += 0.1f)
        {
            i++;
            Vector3 point = startPosition + time * startVelocity;
            point.y = startPosition.y + startVelocity.y * time + (Physics.gravity.y / 2f * time * time);

            (_currentGun as M32A1).LineRenderer.SetPosition(i, point);

            Vector3 lastPosition = (_currentGun as M32A1).LineRenderer.GetPosition(i - 1);

            if (Physics.Raycast(lastPosition, 
                    (point - lastPosition).normalized, 
                    out RaycastHit hit,
                    (point - lastPosition).magnitude,
                    _shootColliderLayerMask))
            {
                (_currentGun as M32A1).LineRenderer.SetPosition(i, hit.point);
                (_currentGun as M32A1).LineRenderer.positionCount = i + 1;
                
                ShowExplosionPreview(hit.point);   
                return;
            }
        }
    }
    
    private void ShowExplosionPreview(Vector3 position)
    {
        if (_aoeIndicatorPrefab == null) return;

        if (_aoeIndicatorInstance == null)
        {
            _aoeIndicatorInstance = Instantiate(_aoeIndicatorPrefab);
            _aoeIndicatorInstance.SetActive(false);
        }

        _aoeIndicatorInstance.transform.position = position;
        Debug.Log(_currentGun.GetBulletRadius());
        _aoeIndicatorInstance.transform.localScale = Vector3.one * _currentGun.GetBulletRadius() * 2; // Scale theo radius
    }
}

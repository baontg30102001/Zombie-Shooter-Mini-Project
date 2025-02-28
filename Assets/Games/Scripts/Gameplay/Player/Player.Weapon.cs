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
    
    private const float _shootAngleThreshold = 45f;

    private void HandlerWeapon()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, _shootColliderLayerMask))
        {
            mouseWorldPosition = hit.point;
        }

        if (_inputSystem.shoot)
        {
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            
            _aimCamera.gameObject.SetActive(true);
            _sensitivity = _shootSensitivity;
            _rotateOnMove = false;
            
            float angleToTarget = Vector3.Angle(transform.forward, aimDirection);

            if (angleToTarget <= _shootAngleThreshold)
            {
                RotateTowardsTarget(aimDirection);
                if (_currentGun != null)
                {
                    _currentGun.Shoot(mouseWorldPosition);
                }
            }
            else
            {
                RotateTowardsTarget(aimDirection);
            }
        }
        else if (!_inputSystem.shoot)
        {
            _aimCamera.gameObject.SetActive(false);
            _sensitivity = _normalSensitivity;
            _rotateOnMove = true;
        }

        if (_inputSystem.reload)
        {
            _currentGun.Reload();
            _inputSystem.reload = false;
        }
    }
    
    private void RotateTowardsTarget(Vector3 aimDirection)
    {
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }
}

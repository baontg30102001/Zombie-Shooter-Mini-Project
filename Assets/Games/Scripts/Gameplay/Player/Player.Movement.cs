using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public partial class Player : MonoBehaviour
{
    [FoldoutGroup("Player Setup"), SerializeField]
    private PlayerInput _playerInput;
    [FoldoutGroup("Player Setup"), SerializeField]
    private Animator _animator;
    [FoldoutGroup("Player Setup"), SerializeField]
    private CharacterController _controller;
    [FoldoutGroup("Player Setup"), SerializeField]
    private InputSystem _inputSystem;
    [FoldoutGroup("Player Setup"), SerializeField]
    private GameObject _mainCamera;
    
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _moveSpeed = 2.0f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _sprintSpeed = 5.335f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _rotationSmoothTime = 0.12f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _speedChangeRate = 10.0f;
    
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _sensitivity = 1.0f;

    [FoldoutGroup("Sounds"), SerializeField]
    private AudioClip _landingAudioClip;
    [FoldoutGroup("Sounds"), SerializeField]
    private AudioClip[] _foostepAudioClips;
    
    [FoldoutGroup("Sounds"), SerializeField, Range(0, 1)]
    private float _footStepAudioVolume = 0.5f;

    [FoldoutGroup("Player Movement"), SerializeField]
    private float _jumpHeight = 1.2f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _gravity = -15.0f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _jumpTimeout = 0.50f;
    [FoldoutGroup("Player Movement"), SerializeField]
    private float _fallTimeout = 0.15f;
    [FoldoutGroup("Player Grounded"), SerializeField]
    private bool _grounded = true;

    [FoldoutGroup("Player Grounded"), SerializeField]
    private float _groundedOffset = -0.14f;
    [FoldoutGroup("Player Grounded"), SerializeField]
    private float _groundedRadius = 0.28f;
    [FoldoutGroup("Player Grounded"), SerializeField]
    private LayerMask _groundLayers;

    [FoldoutGroup("Cinemachine"), SerializeField]
    private GameObject _cinemachineCameraTarget;
    [FoldoutGroup("Cinemachine"), SerializeField]
    private float _topClamp = 70.0f;
    [FoldoutGroup("Cinemachine"), SerializeField]
    private float _bottomClamp = -30.0f;
    [FoldoutGroup("Cinemachine"), SerializeField]
    private float _cameraAngleOverride = 0.0f;
    [FoldoutGroup("Cinemachine"), SerializeField]
    private bool _lockCameraPosition = false;
    
    
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    private int _animIDAim;

    private bool _rotateOnMove = true;
    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private void SetupMovementComponents()
    {
        _cinemachineTargetYaw = _cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = _animator != null;
        
        AssignAnimationIDs();
        
        _jumpTimeoutDelta = _jumpTimeout;
        _fallTimeoutDelta = _fallTimeout;
    }
    
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDAim = Animator.StringToHash("Aim");
    }
    

    private void HandlerMovement()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void JumpAndGravity()
    {
        if (_grounded)
        {
            _fallTimeoutDelta = _fallTimeout;
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
            
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (_inputSystem.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }

        }
        else
        {
            _jumpTimeoutDelta = _jumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            _inputSystem.jump = false;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - _groundedOffset,
            transform.position.z);
        _grounded = Physics.CheckSphere(spherePosition, _groundedRadius, _groundLayers, QueryTriggerInteraction.Ignore);

        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _grounded);
        }
    }

    private void Move()
    {
        float targetSpeed = _inputSystem.sprint ? _sprintSpeed : _moveSpeed;

        if (_inputSystem.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = 1f;

        if (currentHorizontalSpeed < (targetSpeed - speedOffset) ||
            currentHorizontalSpeed > (targetSpeed + speedOffset))
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * _speedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * _speedChangeRate);
        if (_animationBlend < 0.01f)
        {
            _animationBlend = 0f;
        }

        Vector3 inputDirection = new Vector3(_inputSystem.move.x, 0.0f, _inputSystem.move.y).normalized;

        if (_inputSystem.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                _rotationSmoothTime);

            if (_rotateOnMove)
            {
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                         new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
    }

    private void CameraRotation()
    {
        if (_inputSystem.look.sqrMagnitude >= _threshold && !_lockCameraPosition)
        {
            float deltaTimeMultiplier = 1.0f;

            _cinemachineTargetYaw += _inputSystem.look.x * deltaTimeMultiplier * _sensitivity;
            _cinemachineTargetPitch += _inputSystem.look.y * deltaTimeMultiplier * _sensitivity;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _bottomClamp, _topClamp);

        _cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + _cameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (_foostepAudioClips.Length > 0)
            {
                var index = Random.Range(0, _foostepAudioClips.Length);
                AudioSource.PlayClipAtPoint(_foostepAudioClips[index], transform.TransformPoint(_controller.center), _footStepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(_landingAudioClip, transform.TransformPoint(_controller.center), _footStepAudioVolume);
        }
    }
}

using DamageNumbersPro.Demo;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class RangeZombie : Zombie
{
    protected float _attackRangeDistance;
    protected string _gunId;
    protected float _safeDistance;

    [SerializeField] private Transform _gunPosition;
    [SerializeField] private Gun _currentGun;
    [SerializeField] private Animator _animator;
    [SerializeField] private new RangeZombieData _zombieData;
    
    private int _animIDAim;
    
    private EntityIntaller.Settings _entitySettings;
    private M4A1.Factory _m4a1Factory;
    
    [Inject]
    public void Construct(EntityIntaller.Settings entitySettings,
        M4A1.Factory m4a1Factory)
    {
        _entitySettings = entitySettings;
        _m4a1Factory = m4a1Factory;
    }
    public override void InitializeFromData(string zombieId)
    {
        base.InitializeFromData(zombieId);
        _zombieData = _entitySettings.GetRangeZombieDataById(zombieId);

        _zombieId = zombieId;
        _hp = _zombieData.hP;
        _moveSpeed = _zombieData.moveSpeed;
        _detectionRange = _zombieData.detectionRange;
        _attackRangeDistance = _zombieData.attackRangeDistance;
        _gunId = _zombieData.gunId;
        _safeDistance = _zombieData.safeDistance;
        _currentGun = SpawnGuns(_gunId);

        AssignAnimationIDs();
        
        SetState(ZombieStateType.Idle);
    }
    
    protected override void AssignAnimationIDs()
    {
        base.AssignAnimationIDs();
        _animIDAim = Animator.StringToHash("Aim");
    }
    
    private Gun SpawnGuns(string gunId)
    {
        Gun gun = CreateGuns(gunId);
        gun.Initialize(gunId);
        gun.transform.SetParent(_gunPosition, false);
        gun.gameObject.SetActive(true);
        
        return gun;
    }

    private Gun CreateGuns(string gunId) => gunId switch
    {
        GameDefine.GunEntity.M4A1 => _m4a1Factory.Create(),
        //Default
        _ => _m4a1Factory.Create(),
    };
    
    private void OnDeath(AnimationEvent animationEvent)
    {
        gameObject.SetActive(false);
    }

    #region State Machine

    protected override ZombieState CreateState(ZombieStateType stateType)
    {
        Debug.Log(stateType);

        switch (stateType)
        {
            case ZombieStateType.Idle:
                return new IdleState(this);
            case ZombieStateType.Patrolling:
                return new PatrollingState(this);
            case ZombieStateType.Chasing:
                return new ChasingState(this);
            case ZombieStateType.Attacking:
                return new AttackingState(this);
            case ZombieStateType.Avoiding:
                return new AvoidingState(this);
            case ZombieStateType.Dead:
                return new DeadState(this);
            default:
                Debug.LogWarning($"Trạng thái {stateType} không được hỗ trợ bởi RangeZombie.");
                return null;
        }
    }
    
    private class IdleState : ZombieState
    {
        public IdleState(Zombie zombie) : base(zombie, ZombieStateType.Idle) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
            }
        }

        public override void UpdateState()
        {
            if (Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position) < _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        public override void ExitState() { }
    }
    
    private class PatrollingState : ZombieState
    {
        private Vector3 patrolPoint;

        public PatrollingState(Zombie zombie) : base(zombie, ZombieStateType.Patrolling) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = false;
            patrolPoint = GetRandomNavMeshPoint();
            _zombie.GetNavMeshAgent().destination = patrolPoint;
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((RangeZombie)_zombie)._moveSpeed);
            }
        }

        public override void UpdateState()
        {
            if (Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position) < _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
            else if (Vector3.Distance(_zombie.transform.position, patrolPoint) < 1f)
            {
                patrolPoint = GetRandomNavMeshPoint();
                _zombie.GetNavMeshAgent().destination = patrolPoint;
            }
        }

        public override void ExitState() { }

        private Vector3 GetRandomNavMeshPoint()
        {
            NavMeshHit hit;
            NavMesh.SamplePosition(Random.insideUnitSphere * 10f, out hit, 10f, NavMesh.AllAreas);
            return hit.position;
        }
    }
    private class ChasingState : ZombieState
    {
        public ChasingState(Zombie zombie) : base(zombie, ZombieStateType.Chasing) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = false;
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((RangeZombie)_zombie)._moveSpeed);
            }
        }

        public override void UpdateState()
        {
            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            _zombie.GetNavMeshAgent().destination = _zombie.GetPlayer().transform.position;

            if (distanceToPlayer < ((RangeZombie)_zombie)._attackRangeDistance && distanceToPlayer > ((RangeZombie)_zombie)._safeDistance)
            {
                _zombie.SetState(ZombieStateType.Attacking);
            }
            else if (distanceToPlayer <= ((RangeZombie)_zombie)._safeDistance)
            {
                _zombie.SetState(ZombieStateType.Avoiding);
            }
            else if (distanceToPlayer > _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Patrolling);
            }
        }

        public override void ExitState() { }
    }
    private class AttackingState : ZombieState
    {
        private float attackCooldown = 1f; // Có thể thêm vào RangeZombieData nếu cần

        public AttackingState(Zombie zombie) : base(zombie, ZombieStateType.Attacking) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0);
                ((RangeZombie)_zombie)._animator.SetBool(((RangeZombie)_zombie)._animIDAim, true);
            }
        }

        public override void UpdateState()
        {
            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer < ((RangeZombie)_zombie)._safeDistance)
            {
                _zombie.SetState(ZombieStateType.Avoiding);
            }
            else if (distanceToPlayer > ((RangeZombie)_zombie)._attackRangeDistance)
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
            else if(distanceToPlayer <= ((RangeZombie)_zombie)._attackRangeDistance)
            {
                ShootPlayer();
            }
        }

        private void ShootPlayer()
        {
            Player player = _zombie.GetPlayer();
            if (player != null)
            {
                Vector3 worldAimTarget = player.transform.position;
                worldAimTarget.y = _zombie.transform.position.y;
                Vector3 aimDirection = (worldAimTarget - _zombie.transform.position).normalized;

                float angleToTarget = Vector3.Angle(_zombie.transform.forward, aimDirection);

                var playerPosition = new Vector3(
                    player.transform.position.x,
                    player.transform.position.y + player.GetComponent<CharacterController>().height * 0.66f,
                    player.transform.position.z);
                
                if (angleToTarget <= 0.1f)
                {
                    ((RangeZombie)_zombie)._currentGun.Shoot(playerPosition);
                }
                else
                {
                    RotateTowardsTarget(aimDirection);
                }
            }
        }
        
        private void RotateTowardsTarget(Vector3 aimDirection)
        {
            _zombie.transform.forward = Vector3.Lerp(_zombie.transform.forward, aimDirection, Time.deltaTime * 20f);
        }

        public override void ExitState()
        {
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetBool(((RangeZombie)_zombie)._animIDAim, false);
            }
        }
    }
    
    private class AvoidingState : ZombieState
    {
        private Vector3 avoidDirection;

        public AvoidingState(Zombie zombie) : base(zombie, ZombieStateType.Avoiding) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = false;
            avoidDirection = (_zombie.transform.position - _zombie.GetPlayer().transform.position).normalized * 5f;
            Vector3 avoidPoint = _zombie.transform.position + avoidDirection;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(avoidPoint, out hit, 5f, NavMesh.AllAreas))
            {
                _zombie.GetNavMeshAgent().destination = hit.position;
            }
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((RangeZombie)_zombie)._moveSpeed);
            }
        }

        public override void UpdateState()
        {
            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer > ((RangeZombie)_zombie)._safeDistance && distanceToPlayer < ((RangeZombie)_zombie)._attackRangeDistance)
            {
                _zombie.SetState(ZombieStateType.Attacking);
            }
            else if (distanceToPlayer > _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Patrolling);
            }
        }

        public override void ExitState() { }
    }
    
    private class DeadState : ZombieState
    {
        public DeadState(Zombie zombie) : base(zombie, ZombieStateType.Dead) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((RangeZombie)_zombie)._animator != null)
            {
                ((RangeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
                ((RangeZombie)_zombie)._animator.SetTrigger(_zombie.AnimIdDeath);
            }
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }

    #endregion 
    

    public class Factory : PlaceholderFactory<RangeZombie>
    {
        
    }
}
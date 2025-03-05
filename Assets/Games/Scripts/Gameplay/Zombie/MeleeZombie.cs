using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class MeleeZombie : Zombie
{
    protected float _damage;
    protected float _attackMeleeDistance;
    
    [SerializeField] private Animator _animator;
    [SerializeField] private MeleeZombieData _zombieData;
    
    private EntityIntaller.Settings _entitySettings;

    private int _animIDAttack;
    public Animator Animator => _animator;
    public float GetHP() => _hp;
    
    [Inject]
    public void Construct(EntityIntaller.Settings entitySettings)
    {
        _entitySettings = entitySettings;
    }
    
    public override void InitializeFromData(string zombieId)
    {
        base.InitializeFromData(zombieId);
        _zombieData = _entitySettings.GetMeleeZombieDataById(zombieId);

        _zombieId = zombieId;
        _hp = _zombieData.hP;
        _moveSpeed = _zombieData.moveSpeed;
        _damage = _zombieData.damage;
        _detectionRange = _zombieData.detectionRange;
        _attackMeleeDistance = _zombieData.attackMeleeDistance;

        AssignAnimationIDs();
        
        SetState(ZombieStateType.Idle);
    }

    protected override void AssignAnimationIDs()
    {
        base.AssignAnimationIDs();
        _animIDAttack = Animator.StringToHash("Attack");
    }

    private void OnAttack(AnimationEvent animationEvent)
    {
        AttackPlayer();
    }

    private void OnDeath(AnimationEvent animationEvent)
    {
        gameObject.SetActive(false);
        _gameplayManager.RecheckZombies();
    }
    
    private void AttackPlayer()
    {
        Player playerScript = GetPlayer();
        if (playerScript != null)
        {
            playerScript.TakeDamage(_damage);
        }
    }

    protected override ZombieState CreateState(ZombieStateType stateType)
    {
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
            case ZombieStateType.Dead:
                return new DeadState(this);
            default:
                Debug.LogWarning($"Trạng thái {stateType} không được hỗ trợ bởi MeleeZombie.");
                return null;
        }
    }
    
    private class IdleState : ZombieState
    {
        public IdleState(Zombie zombie) : base(zombie, ZombieStateType.Idle) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true; // Dừng di chuyển
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
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
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((MeleeZombie)_zombie)._moveSpeed);
            }
        }

        public override void UpdateState()
        {
            if (Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position) < _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
            else if (Vector3.Distance(_zombie.transform.position, patrolPoint) < 1f) // Đến điểm patrol
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
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((MeleeZombie)_zombie)._moveSpeed);
            }
        }

        public override void UpdateState()
        {
            _zombie.GetNavMeshAgent().destination = _zombie.GetPlayer().transform.position;

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer < ((MeleeZombie)_zombie)._attackMeleeDistance)
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

    private class AttackingState : ZombieState
    {
        private float _lastAttackTime;

        public AttackingState(Zombie zombie) : base(zombie, ZombieStateType.Attacking) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
            }
            _lastAttackTime = Time.time;
        }

        public override void UpdateState()
        {
            
            Vector3 worldAimTarget = _zombie.GetPlayer().transform.position;
            worldAimTarget.y = _zombie.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - _zombie.transform.position).normalized;

            RotateTowardsTarget(aimDirection);
                
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetBool(((MeleeZombie)_zombie)._animIDAttack, true);
            }

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer > ((MeleeZombie)_zombie)._attackMeleeDistance)
            {
                if (((MeleeZombie)_zombie)._animator != null)
                {
                    ((MeleeZombie)_zombie)._animator.SetBool(((MeleeZombie)_zombie)._animIDAttack, false);
                }
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        public override void ExitState()
        {
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetBool(((MeleeZombie)_zombie)._animIDAttack, false);
            }
        }
        private void RotateTowardsTarget(Vector3 aimDirection)
        {
            _zombie.transform.forward = Vector3.Lerp(_zombie.transform.forward, aimDirection, Time.deltaTime * 20f);
        }
    }

    private class DeadState : ZombieState
    {
        public DeadState(Zombie zombie) : base(zombie, ZombieStateType.Dead) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((MeleeZombie)_zombie)._animator != null)
            {
                ((MeleeZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
                ((MeleeZombie)_zombie)._animator.SetTrigger(_zombie.AnimIdDeath);
            }
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }
    
    public class Factory : PlaceholderFactory<MeleeZombie>
    {
        
    }
}
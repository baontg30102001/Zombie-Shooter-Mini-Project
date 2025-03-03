using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class MeleeZombie : Zombie
{
    protected float _damage;
    protected float _attackMeleeDistance;
    protected float _cooldownMeleeAttack;
    
    [SerializeField] private MeleeZombieData _zombieData;
    
    private EntityIntaller.Settings _entitySettings;

    [Inject]
    public void Construct(EntityIntaller.Settings entitySettings)
    {
        _entitySettings = entitySettings;
    }
    
    public override void InitializeFromData(string zombieId)
    {
        base.InitializeFromData(zombieId);

        _zombieData = _entitySettings.GetMeleeZombieDataById(zombieId);
        _damage = _zombieData.damage;
        _attackMeleeDistance = _zombieData.attackMeleeDistance;
        _cooldownMeleeAttack = _zombieData.cooldownMeleeAttack;
        SetState(ZombieStateType.Idle);
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
            _lastAttackTime = Time.time;
        }

        public override void UpdateState()
        {
            if (Time.time - _lastAttackTime >= ((MeleeZombie)_zombie)._cooldownMeleeAttack)
            {
                AttackPlayer();
                _lastAttackTime = Time.time;
            }

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer > ((MeleeZombie)_zombie)._attackMeleeDistance)
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        private void AttackPlayer()
        {
            Player playerScript = _zombie.GetPlayer();
            if (playerScript != null)
            {
                playerScript.TakeDamage(((MeleeZombie)_zombie)._damage);
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
            _zombie.gameObject.SetActive(false); // Hoặc thêm hiệu ứng chết
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }
    
    public class Factory : PlaceholderFactory<MeleeZombie>
    {
        
    }
}
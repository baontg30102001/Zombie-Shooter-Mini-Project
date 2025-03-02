using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class RangeZombie : Zombie
{
    protected float _attackRangeDistance;
    protected string _gunId;
    protected float _safeDistance;
    
    [SerializeField] private new RangeZombieData _zombieData;
    
    private EntityIntaller.Settings _entitySettings;

    [Inject]
    public void Construct(EntityIntaller.Settings entitySettings)
    {
        _entitySettings = entitySettings;
    }
    public override void InitializeFromData(string zombieId)
    {
        base.InitializeFromData(zombieId);

        _zombieData = _entitySettings.GetRangeZombieDataById(zombieId);
        _attackRangeDistance = _zombieData.attackRangeDistance;
        _gunId = _zombieData.gunId;
        _safeDistance = _zombieData.safeDistance;
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
        private float lastAttackTime;
        private float attackCooldown = 1f; // Có thể thêm vào RangeZombieData nếu cần

        public AttackingState(Zombie zombie) : base(zombie, ZombieStateType.Attacking) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            lastAttackTime = Time.time;
        }

        public override void UpdateState()
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                ShootPlayer();
                lastAttackTime = Time.time;
            }

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer < ((RangeZombie)_zombie)._safeDistance)
            {
                _zombie.SetState(ZombieStateType.Avoiding);
            }
            else if (distanceToPlayer > ((RangeZombie)_zombie)._attackRangeDistance)
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        private void ShootPlayer()
        {
            Player playerScript = _zombie.GetPlayer();
            if (playerScript != null)
            {
                playerScript.TakeDamage(10f); // Giả định sát thương, có thể lấy từ hệ thống vũ khí dựa trên gunId
            }
        }

        public override void ExitState() { }
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
            _zombie.gameObject.SetActive(false);
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }

    public class Factory : PlaceholderFactory<RangeZombie>
    {
        
    }
}
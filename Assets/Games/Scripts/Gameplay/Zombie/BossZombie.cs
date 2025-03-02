using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Zenject;

public class BossZombie : Zombie
{
    protected string _gunId;
    protected float _damage;
    protected float _cooldownMeleeAttack;
    protected float _aoeRadius;
    protected float _aoeDamage;
    protected float _aoePreparationTime;
    protected float _stunDuration;
    
    [SerializeField] private new BossZombieData _zombieData;
    
    private EntityIntaller.Settings _entitySettings;

    [Inject]
    public void Construct(EntityIntaller.Settings entitySettings)
    {
        _entitySettings = entitySettings;
    }
    
    public override void InitializeFromData(string zombieId)
    {
        base.InitializeFromData(zombieId);

        _zombieData = _entitySettings.GetBossZombieDataById(zombieId);
        
        _gunId = _zombieData.gunId;
        _damage = _zombieData.damage;
        _cooldownMeleeAttack = _zombieData.cooldownMeleeAttack;
        _aoeRadius = _zombieData.aoeRadius;
        _aoeDamage = _zombieData.aoeDamage;
        _aoePreparationTime = _zombieData.aoePreparationTime;
        _stunDuration = _zombieData.stunDuration;
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
            case ZombieStateType.PreparingAOE:
                return new PreparingAOEState(this);
            case ZombieStateType.Dead:
                return new DeadState(this);
            default:
                Debug.LogWarning($"Trạng thái {stateType} không được hỗ trợ bởi Boss.");
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
            _zombie.GetNavMeshAgent().destination = _zombie.GetPlayer().transform.position;

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer < _zombie.GetDetectionRange() / 2) // Giả định tấn công gần
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
        private float lastAttackTime;

        public AttackingState(Zombie zombie) : base(zombie, ZombieStateType.Attacking) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            lastAttackTime = Time.time;
        }

        public override void UpdateState()
        {
            if (Time.time - lastAttackTime >= ((BossZombie)_zombie)._cooldownMeleeAttack)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }

            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            if (distanceToPlayer > _zombie.GetDetectionRange() / 2)
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
            else if (Random.value < 0.1f) // 10% cơ hội kích hoạt AOE
            {
                _zombie.SetState(ZombieStateType.PreparingAOE);
            }
        }

        private void AttackPlayer()
        {
            Player playerScript = _zombie.GetPlayer();
            if (playerScript != null)
            {
                playerScript.TakeDamage(((BossZombie)_zombie)._damage);
            }
        }

        public override void ExitState() { }
    }
    
    private class PreparingAOEState : ZombieState
    {
        private float timer;

        public PreparingAOEState(Zombie zombie) : base(zombie, ZombieStateType.PreparingAOE) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            timer = 0f;
            ShowAOEIndicator();
        }

        public override void UpdateState()
        {
            timer += Time.deltaTime;
            if (timer >= ((BossZombie)_zombie)._aoePreparationTime)
            {
                TriggerAOE();
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        private void ShowAOEIndicator()
        {
            Debug.Log("Hiển thị chỉ báo AOE với bán kính " + ((BossZombie)_zombie)._aoeRadius);
        }

        private void TriggerAOE()
        {
            Collider[] hitColliders = Physics.OverlapSphere(_zombie.transform.position, ((BossZombie)_zombie)._aoeRadius);
            foreach (Collider hit in hitColliders)
            {
                Player playerScript = hit.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(((BossZombie)_zombie)._aoeDamage);
                    playerScript.Stun(((BossZombie)_zombie)._stunDuration);
                }
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
    
    public class Factory : PlaceholderFactory<BossZombie>
    {
        
    }
}
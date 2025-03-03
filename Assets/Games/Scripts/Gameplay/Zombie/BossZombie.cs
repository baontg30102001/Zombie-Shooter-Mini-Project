using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Zenject;

public class BossZombie : Zombie
{
    protected string _gunId;
    protected float _damage;
    protected float _cooldownMeleeAttack;
    protected float _attackMeleeDistance;
    protected float _attackRangeDistance;
    protected float _aoeRadius;
    protected float _aoeDamage;
    protected float _aoePreparationTime;
    protected float _cooldownSkill;
    
    [SerializeField] private Transform _gunPosition;
    [SerializeField] private Gun _currentGun;
    [SerializeField] private new BossZombieData _zombieData;
    
    [SerializeField] private GameObject _aoeIndicator;
    
    private EntityIntaller.Settings _entitySettings;
    private M4A1.Factory _m4a1Factory;
    protected float _lastUsingSkill;

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

        _zombieData = _entitySettings.GetBossZombieDataById(zombieId);
        
        _gunId = _zombieData.gunId;
        _damage = _zombieData.damage;
        _attackMeleeDistance = _zombieData.attackMeleeDistance;
        _attackRangeDistance = _zombieData.attackRangeDistance;
        _cooldownMeleeAttack = _zombieData.cooldownMeleeAttack;
        _aoeRadius = _zombieData.aoeRadius;
        _aoeDamage = _zombieData.aoeDamage;
        _aoePreparationTime = _zombieData.aoePreparationTime;
        _cooldownSkill = _zombieData.cooldownSkill;
        
        _currentGun = SpawnGuns(_gunId);
        SetState(ZombieStateType.Idle);
    }
    
    private Gun SpawnGuns(string gunId)
    {
        Gun gun = CreateGuns(gunId);
        gun.Initialize(gunId);
        gun.transform.SetParent(_gunPosition, false);
        
        return gun;
    }

    private Gun CreateGuns(string gunId) => gunId switch
    {
        GameDefine.GunEntity.M4A1 => _m4a1Factory.Create(),
        //Default
        _ => _m4a1Factory.Create(),
    };

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
            if (distanceToPlayer <= ((BossZombie)_zombie)._attackRangeDistance) // Giả định tấn công gần
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
            float distanceToPlayer = Vector3.Distance(_zombie.transform.position, _zombie.GetPlayer().transform.position);
            
            if (Random.value < 0.01f) // 1% cơ hội kích hoạt AOE
            {
                Debug.Log("Thời gian hồi còn: " + (((BossZombie)_zombie)._cooldownSkill - (Time.time - ((BossZombie)_zombie)._lastUsingSkill)));
                if (Time.time - ((BossZombie)_zombie)._lastUsingSkill >= ((BossZombie)_zombie)._cooldownSkill)
                {
                    _zombie.SetState(ZombieStateType.PreparingAOE);
                }
            }

            if (distanceToPlayer <= ((BossZombie)_zombie)._attackMeleeDistance)
            {
                if (Time.time - lastAttackTime >= ((BossZombie)_zombie)._cooldownMeleeAttack)
                {
                    Debug.Log("Melee");
                    AttackPlayer();
                    lastAttackTime = Time.time;
                }
            }
            else if(distanceToPlayer <= ((BossZombie)_zombie)._attackRangeDistance)
            {
                ShootPlayer();
            }
            else if(distanceToPlayer <= _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Chasing);
            }
        }

        private void AttackPlayer()
        {
            Player player = _zombie.GetPlayer();
            if (player != null)
            {
                player.TakeDamage(((BossZombie)_zombie)._damage);
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
                    ((BossZombie)_zombie)._currentGun.Shoot(playerPosition);
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

        public override void ExitState() { }
    }
    
    private class PreparingAOEState : ZombieState
    {
        private float _timer;

        public PreparingAOEState(Zombie zombie) : base(zombie, ZombieStateType.PreparingAOE) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            _timer = 0f;
            ShowAOEIndicator();
            ((BossZombie)_zombie)._lastUsingSkill = Time.time;
        }

        public override void UpdateState()
        {
            _timer += Time.deltaTime;
            
            if (((BossZombie)_zombie)._aoeIndicator != null)
            {
                float progress = _timer / ((BossZombie)_zombie)._aoePreparationTime;
                float scale = Mathf.Lerp(0.1f, ((BossZombie)_zombie)._aoeRadius * 2, progress);
                ((BossZombie)_zombie)._aoeIndicator.transform.localScale = new Vector3(scale, 0.01f, scale);
            }
            
            if (_timer >= ((BossZombie)_zombie)._aoePreparationTime)
            {
                TriggerAOE();
                _zombie.SetState(ZombieStateType.Idle);
            }
        }

        private void ShowAOEIndicator()
        {
            ((BossZombie)_zombie)._aoeIndicator.SetActive(true);
            ((BossZombie)_zombie)._aoeIndicator.transform.localScale = Vector3.zero;
        }

        private void TriggerAOE()
        {
            if (((BossZombie)_zombie)._aoeIndicator != null)
            {
                ((BossZombie)_zombie)._aoeIndicator.SetActive(false);
            }
            
            Collider[] hitColliders = Physics.OverlapSphere(_zombie.transform.position, ((BossZombie)_zombie)._aoeRadius);
            foreach (Collider hit in hitColliders)
            {
                Player player = hit.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamage(((BossZombie)_zombie)._aoeDamage);
                    // player.Stun(((BossZombie)_zombie)._stunDuration);
                }
            }
        }

        public override void ExitState()
        {
            ((BossZombie)_zombie)._aoeIndicator.SetActive(false);
        }
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
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
    [SerializeField] private Animator _animator;
    [SerializeField] private new BossZombieData _zombieData;
    
    [SerializeField] private GameObject _aoeIndicator;

    private int _animIDAim;
    private int _animIDSkill;
    private int _animIDAttack;
    
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

        _zombieId = zombieId;
        _hp = _zombieData.hP;
        _moveSpeed = _zombieData.moveSpeed;
        _detectionRange = _zombieData.detectionRange;
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
        
        AssignAnimationIDs();
        
        SetState(ZombieStateType.Idle);
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

    private void OnAttack(AnimationEvent animationEvent)
    {
        AttackPlayer();
    }

    private void OnDeath(AnimationEvent animationEvent)
    {
        gameObject.SetActive(false);
    }

    private void AttackPlayer()
    {
        Player playerScript = GetPlayer();
        if (playerScript != null)
        {
            playerScript.TakeDamage(_damage);
        }
    }
    
    protected override void AssignAnimationIDs()
    {
        base.AssignAnimationIDs();
        _animIDAttack = Animator.StringToHash("Attack");
        _animIDAim = Animator.StringToHash("Aim");
        _animIDSkill = Animator.StringToHash("Skill");
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
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
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
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((BossZombie)_zombie)._moveSpeed);
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
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, ((BossZombie)_zombie)._moveSpeed);
            }
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
        public AttackingState(Zombie zombie) : base(zombie, ZombieStateType.Attacking) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0);
            }
        }

        public override void UpdateState()
        {
            Vector3 worldAimTarget = _zombie.GetPlayer().transform.position;
            worldAimTarget.y = _zombie.transform.position.y;
            Vector3 aimDirection = (worldAimTarget - _zombie.transform.position).normalized;
            
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
                RotateTowardsTarget(aimDirection);

                if (((BossZombie)_zombie)._animator != null)
                {
                    ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAttack, true);
                    ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAim, false);
                }
            }
            else if(distanceToPlayer <= ((BossZombie)_zombie)._attackRangeDistance)
            {
                if (((BossZombie)_zombie)._animator != null)
                {
                    ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAttack, false);
                    ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAim, true);
                }
                ShootPlayer();
            }
            else if(distanceToPlayer <= _zombie.GetDetectionRange())
            {
                _zombie.SetState(ZombieStateType.Chasing);
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

        public override void ExitState()
        {
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAttack, false);
                ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDAim, false);
            }
        }
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
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0);
                ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDSkill, true);
            }
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
                }
            }
        }

        public override void ExitState()
        {
            ((BossZombie)_zombie)._aoeIndicator.SetActive(false);
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0);
                ((BossZombie)_zombie)._animator.SetBool(((BossZombie)_zombie)._animIDSkill, false); ;
            }
        }
    }
    private class DeadState : ZombieState
    {
        public DeadState(Zombie zombie) : base(zombie, ZombieStateType.Dead) { }

        public override void EnterState()
        {
            _zombie.GetNavMeshAgent().isStopped = true;
            if (((BossZombie)_zombie)._animator != null)
            {
                ((BossZombie)_zombie)._animator.SetFloat(_zombie.AnimIdSpeed, 0f);
                ((BossZombie)_zombie)._animator.SetTrigger(_zombie.AnimIdDeath);
            }
        }

        public override void UpdateState() { }

        public override void ExitState() { }
    }
    
    public class Factory : PlaceholderFactory<BossZombie>
    {
        
    }
}
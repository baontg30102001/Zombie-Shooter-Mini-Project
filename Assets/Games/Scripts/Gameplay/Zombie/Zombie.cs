using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public enum ZombieType
{
    Melee,
    Range,
    Boss
}

public abstract class Zombie : MonoBehaviour
{
    protected string _zombieId;
    protected float _hp = 100f;
    protected float _moveSpeed;
    protected float _detectionRange = 20f;

    [SerializeField] protected NavMeshAgent _navMeshAgent;
    [SerializeField] protected Player _player;
    
    protected ZombieState _currentState;

    private int _animIDSpeed;
    private int _animIDDeath;
    
    public float GetDetectionRange() => _detectionRange;
    public NavMeshAgent GetNavMeshAgent() => _navMeshAgent;
    public Player GetPlayer() => _player;
    public int AnimIdSpeed => _animIDSpeed;
    public int AnimIdDeath => _animIDDeath;
    
    protected void Update()
    {
        if (_hp <= 0)
        {
            SetState(ZombieStateType.Dead);
            return;
        }
        _currentState?.UpdateState();
    }
        
    public void SetState(ZombieStateType stateType)
    {
        if (_currentState?.StateType == stateType)
            return;
        _currentState?.ExitState();
        _currentState = CreateState(stateType);
        _currentState?.EnterState(); 
    }
    
    public virtual void InitializeFromData(string zombieId)
    {
        
    }
    
    protected virtual void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDDeath = Animator.StringToHash("Death");
    }

    public virtual void TakeDamage(float damage)
    {
        Debug.Log(damage);
        _hp -= (int)damage;
    }
    
    protected abstract ZombieState CreateState(ZombieStateType stateType);

}
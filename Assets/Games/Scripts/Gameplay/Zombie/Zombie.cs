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
    protected float _hp = 100f;
    protected float _detectionRange = 20f;

    [SerializeField] protected NavMeshAgent _navMeshAgent;
    [SerializeField] protected Player _player;
    
    protected ZombieState _currentState;
    
    public float GetDetectionRange() => _detectionRange;
    public NavMeshAgent GetNavMeshAgent() => _navMeshAgent;
    public Player GetPlayer() => _player;

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
        _currentState?.ExitState();
        _currentState = CreateState(stateType);
        _currentState?.EnterState();
    }
    
    public virtual void InitializeFromData(string zombieId)
    {
        
    }
    
    protected abstract ZombieState CreateState(ZombieStateType stateType);

}
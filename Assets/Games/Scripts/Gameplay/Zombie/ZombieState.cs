using System;
using UnityEngine;

[Serializable]
public abstract class ZombieState
{
    protected Zombie _zombie;
    protected ZombieStateType _stateType;

    public ZombieStateType StateType
    {
        get => _stateType;
        protected set => _stateType = value;
    }

    protected ZombieState(Zombie zombie, ZombieStateType stateType)
    {
        _zombie = zombie;
        this.StateType = stateType;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    
}

public enum ZombieStateType
{
    Idle,
    Patrolling,
    Chasing,
    Attacking,
    Avoiding, // Chỉ dành cho RangeZombie
    PreparingAOE, // Chỉ dành cho Boss
    Dead
}
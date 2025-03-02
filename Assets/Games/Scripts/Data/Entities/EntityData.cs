using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class EntityData
{
    public string entityId;
    public float hP;
    public float moveSpeed; 
}

[Serializable]
public class PlayerData : EntityData
{
    public float sprintSpeed;
    public float jumpHeight;
    public float sensitivity;
    public List<string> guns;
}

[Serializable]
public class ZombieData : EntityData
{
    public float detectionRange;
}

[Serializable]
public class MeleeZombieData : ZombieData
{
    public float attackMeleeDistance;
    public float damage;
    public float cooldownMeleeAttack;
}

[Serializable]
public class RangeZombieData : ZombieData
{
    [FormerlySerializedAs("attackRangerRange")] public float attackRangeDistance;
    public float safeDistance;
    public string gunId;
}

[Serializable]
public class BossZombieData : ZombieData
{
    public string gunId;
    public float damage;
    public float cooldownMeleeAttack;
    public float aoeRadius;
    public float aoeDamage;
    public float aoePreparationTime;
    public float stunDuration;
}


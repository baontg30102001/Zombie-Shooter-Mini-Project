using System;
using System.Collections.Generic;

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
    
}

[Serializable]
public class MeleeZombieData : ZombieData
{
    public float damage;
}

[Serializable]
public class RangeZombieData : ZombieData
{
    public string gunId;
}

[Serializable]
public class BossZombieData : ZombieData
{
    public string gunId;
    public float damage;
}


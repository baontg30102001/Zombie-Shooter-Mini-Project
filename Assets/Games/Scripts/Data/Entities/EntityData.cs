using System;

[Serializable]
public class EntityData
{
    public string entityId;
    public int hP;
    public int moveSpeed; 
}

[Serializable]
public class PlayerData : EntityData
{
    public int sprintSpeed;
    public int jumpHeight;
}

[Serializable]
public class ZombieData : EntityData
{
    
}

[Serializable]
public class MeleeZombieData : ZombieData
{
    public int damage;
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
    public int damage;
}


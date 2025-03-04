using System.Collections.Generic;
using UnityEngine;
using Zenject;

public partial class Player : MonoBehaviour
{
    [SerializeField] private float _hp = 100;
    [SerializeField] private float _maxHP = 100;
    [SerializeField] private Gun _currentGun;
    [SerializeField] private List<Gun> _gunInInventory;
    [SerializeField] private List<string> _inventory;
    [SerializeField] private UIGameplay _uiGameplay;
    
    private EntityIntaller.Settings _entityConfig;
    private PlayerData _playerData = new PlayerData();

    #region Gun Entity

    private M4A1.Factory _m4a1Factory;
    private M32A1.Factory _m32a1Factory;

    #endregion

    public List<string> Inventory
    {
        get => _inventory;
        set => _inventory = value;
    }
    public float GetPlayerHP() => _hp;
    public float GetPlayerMaxHP() => _maxHP;
    public Gun CurrentGun
    {
        get => _currentGun;
        set
        {
            if (_currentGun != null)
            {
                _currentGun.gameObject.SetActive(false);
            }
            _currentGun = value;
            _currentGun.gameObject.SetActive(true);
        }
    }

    public UIGameplay GetUIGameplay() => _uiGameplay;
    
    [Inject]
    public void Construct(
        EntityIntaller.Settings entityConfig,
        M4A1.Factory m4a1Factory,
        M32A1.Factory m32a1Factory
        )
    {
        _entityConfig = entityConfig;
        _m4a1Factory = m4a1Factory;
        _m32a1Factory = m32a1Factory;
    }
    public void Initialize()
    {
        _playerData = _entityConfig.PlayerData;

        _hp = _playerData.hP;
        _maxHP = _playerData.hP;
        _moveSpeed = _playerData.moveSpeed;
        _sprintSpeed = _playerData.sprintSpeed;
        _jumpHeight = _playerData.jumpHeight;
        _sensitivity = _playerData.sensitivity;

        if (_playerData.guns != null)
        {
            foreach (var gunId in _playerData.guns)
            {
                Gun gun = SpawnGuns(gunId);
                
                _gunInInventory.Add(gun);
            }
            
            CurrentGun = _gunInInventory[0];
        }
    }
    
    private void Start()
    {
        SetupMovementComponents();
    }

    private void Update()
    {
        HandlerWeapon();
        HandlerMovement();
    }

    private Gun SpawnGuns(string gunId)
    {
        Gun gun = CreateGuns(gunId);
        gun.Initialize(gunId);
        gun.transform.SetParent(_gunPosition, false);
        gun.gameObject.SetActive(false);
        gun.SetPlayer(this);
        
        return gun;
    }

    private Gun CreateGuns(string gunId) => gunId switch
    {
        GameDefine.GunEntity.M4A1 => _m4a1Factory.Create(),
        GameDefine.GunEntity.M32A1 => _m32a1Factory.Create(),
        //Default
        _ => _m4a1Factory.Create(),
    };

    public void TakeDamage(float damage)
    {
        _hp -= damage;
        if (_hp <= 0)
        {
            _hp = 0;
        }
    }

    public void Stun(float duration)
    {
        
    }
    
    
    public class Factory : PlaceholderFactory<Player>
    {
        
    }
}

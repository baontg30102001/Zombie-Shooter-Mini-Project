using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

public partial class Player : MonoBehaviour
{
    [SerializeField] private float _health = 100;
    [SerializeField] private Gun _currentGun;
    [SerializeField] private List<Gun> _gunInInventory;
    [SerializeField] private Dictionary<string, int> _inventory;
    
    private EntityIntaller.Settings _entityConfig;
    private PlayerData _playerData = new PlayerData();

    #region Gun Entity

    private M4A1.Factory _m4a1Factory;
    private M32A1.Factory _m32a1Factory;

    #endregion

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

        _health = _playerData.hP;
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
        
        return gun;
    }

    private Gun CreateGuns(string gunId) => gunId switch
    {
        GameDefine.GunEntity.M4A1 => _m4a1Factory.Create(),
        GameDefine.GunEntity.M32A1 => _m32a1Factory.Create(),
        //Default
        _ => _m4a1Factory.Create(),
    };

    private void ChangeGun()
    {
        
    }
    
    public class Factory : PlaceholderFactory<Player>
    {
        
    }
}

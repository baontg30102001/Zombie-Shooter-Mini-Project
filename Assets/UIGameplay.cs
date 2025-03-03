using TMPro;
using UnityEngine;

public class UIGameplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _healthPlayer;
    [SerializeField] private TextMeshProUGUI _magazineGun;
    [SerializeField] private Player _player;
    
    [SerializeField] private int _ammo;
    [SerializeField] private int _magazineMax;
    [SerializeField] private float _hp;
    [SerializeField] private float _maxHp;
    
    private void Update()
    {
        UpdateInfoHero();
    }

    private void UpdateInfoHero()
    {
        _hp = _player.GetPlayerHP();
        _maxHp = _player.GetPlayerMaxHP();

        _ammo = _player.CurrentGun.GetCurrentAmmo();
        _magazineMax = _player.CurrentGun.GetAmmoRemaining();

        _healthPlayer.text = $"{_hp}/{_maxHp}";
        _magazineGun.text = $"{_ammo}/{_magazineMax}";
    }

}

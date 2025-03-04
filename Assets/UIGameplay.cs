using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIGameplay : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _magazineGun;
    [SerializeField] private Player _player;
    [SerializeField] private Image _gunIcon;
    [SerializeField] private Image _reloadImage;
    [SerializeField] private int _ammo;
    [SerializeField] private int _magazineMax;
    [SerializeField] private float _hp;
    [SerializeField] private float _maxHp;

    public Image ReloadImage => _reloadImage;
    
    private GunInstaller.Settings _gunSetting;

    [Inject]
    public void Construct(GunInstaller.Settings gunSetting)
    {
        _gunSetting = gunSetting;
    }
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

        _slider.maxValue = _maxHp;
        _slider.value = _hp;

        _gunIcon.sprite = _gunSetting.GetGunDataById(_player.CurrentGun.GetGunId()).gunImage;
        _magazineGun.text = $"{_ammo}/{_magazineMax}";
    }

}

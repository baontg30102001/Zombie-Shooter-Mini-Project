using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private GameObject _panelEndGame;
    [SerializeField] private GameObject _winIcon;
    [SerializeField] private GameObject _loseIcon;
    [SerializeField] private Button _home;
    [SerializeField] private TextMeshProUGUI _notice;
    public Image ReloadImage => _reloadImage;

    public TextMeshProUGUI Notice
    {
        get => _notice;
        set => _notice = value;
    }
    
    private GunInstaller.Settings _gunSetting;

    [Inject]
    public void Construct(GunInstaller.Settings gunSetting)
    {
        _gunSetting = gunSetting;
    }

    private void Start()
    {
        _home.onClick.AddListener(LoadGameWithLoadingScreen);
    }

    private void Update()
    {
        UpdateInfoHero();
    }
    
    public void LoadGameWithLoadingScreen()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);

        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.UnloadSceneAsync("Gameplay");
        SceneManager.UnloadSceneAsync("LoadingScene");
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

    public void GameWinOrLose(bool isWin)
    {
        _panelEndGame.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        _winIcon.SetActive(isWin);
        _loseIcon.SetActive(!isWin);
    }

    private void OnDestroy()
    {
        _home.onClick.RemoveAllListeners();
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeScene : MonoBehaviour
{
    [SerializeField] private Button _buttonNewGame;
    [SerializeField] private Button _buttonQuitGame;

    private void Start()
    {
        _buttonNewGame.onClick.AddListener(OnButtonNewGame);
        _buttonQuitGame.onClick.AddListener(OnButtonQuitGame);
    }

    private void OnButtonNewGame()
    {
        LoadGameWithLoadingScreen();
    }
    
    public void LoadGameWithLoadingScreen()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);

        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Gameplay", LoadSceneMode.Additive);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        SceneManager.UnloadSceneAsync("Home");
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    private void OnButtonQuitGame()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        _buttonNewGame.onClick.RemoveAllListeners();
        _buttonQuitGame.onClick.RemoveAllListeners();
    }
}

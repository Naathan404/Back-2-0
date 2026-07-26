using System;
using MeowgaByte.Core;
using UnityEditor;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string _levelToLoad = "Level_1";
    [SerializeField] private GameObject _credit;
    public void LoadGame()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.ButtonClick, true);
        SceneController.Instance.LoadSceneWithName(_levelToLoad);
    }

    private void Start()
    {
        _credit.SetActive(false);
    }

    public void OpenCredit()
    {
        _credit.SetActive(true);
    }

    public void CloseCredit()
    {
        _credit.SetActive(false);
    }

    public void Quit()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.ButtonClick, true);
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

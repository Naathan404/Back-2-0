using MeowgaByte.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _settingPanel;
    [SerializeField] private GameObject _settingButton;
    private GameState _gameState;

    [SerializeField] private Image _sfxButton;
    [SerializeField] private Image _bgmButton;

    private void Start()
    {
        UpdateButton();
        _settingButton.SetActive(true);
        _settingPanel.SetActive(false);
    }

    private void UpdateButton()
    {
        _bgmButton.color = AudioManager.Instance.IsBGMMuted ? Color.red : Color.white;
        _sfxButton.color = AudioManager.Instance.IsSFXMuted ? Color.red : Color.white;
    }

    public void BGM()
    {
        AudioManager.Instance.ToggleMuteBGM();
        UpdateButton();    
    }

    public void SFX()
    {
        AudioManager.Instance.ToggleMuteSFX();
        UpdateButton();    
    }

    public void OpenSetting()
    {
        _settingPanel.SetActive(true);
        _settingButton.SetActive(false);
        _gameState = GameManager.Instance.State;
        GameManager.Instance.State = GameState.Pause;
    }

    public void CloseSetting()
    {
        _settingPanel.SetActive(false);
        _settingButton.SetActive(true);
        GameManager.Instance.State = _gameState;
    }

    public void BackToMainMenu()
    {
        SceneController.Instance.LoadSceneMosaicWipe("Menu");
    }
}

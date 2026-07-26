using DG.Tweening;
using MeowgaByte.Core;
using TMPro;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private TextMeshProUGUI _snapIntervalText;
    [SerializeField] private TextMeshProUGUI _notifyText;

    private void Start()
    {
        _notifyText.text = "";
    }

    private bool _isPlaying = false;
    
    public void UpdateCounterText(float time)
    {
        _counterText.text = time.ToString("N2");
    }

    public void UpdateSnapIntervalText(float time)
    {
        _snapIntervalText.text = time.ToString("N2");
    }

    public void UpdateNotifyText(string act)
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.HitCommandSFX, true);
        _notifyText.transform.DOKill();
        _notifyText.text = act;
        _notifyText.transform.DOPunchScale(0.5f * Vector2.one, 0.3f);
    }
}

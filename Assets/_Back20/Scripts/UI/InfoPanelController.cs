using TMPro;
using UnityEngine;

public class InfoPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _counterText;
    [SerializeField] private TextMeshProUGUI _snapIntervalText;

    private bool _isPlaying = false;
    
    public void UpdateCounterText(float time)
    {
        _counterText.text = time.ToString("N2");
    }

    public void UpdateSnapIntervalText(float time)
    {
        _snapIntervalText.text = time.ToString("N2");
    }
}

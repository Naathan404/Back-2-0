using System;
using DG.Tweening;
using MeowgaByte.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverUIObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2 _originalScale;
    [SerializeField] private float _punchDuration = 0.1f;
    [SerializeField] private float _scaleAmount = 1.2f;

    [SerializeField] private bool _playSound = true;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_playSound)
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.Hover, true);
        transform.DOKill();
        transform.DOScale(_originalScale * _scaleAmount, _punchDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_originalScale, _punchDuration);
    }
}

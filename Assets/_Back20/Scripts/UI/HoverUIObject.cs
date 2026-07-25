using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverUIObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector2 _originalScale;
    [SerializeField] private float _punchDuration = 0.1f;
    [SerializeField] private float _scaleAmount = 1.2f;

    private void Start()
    {
        _originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(_originalScale * _scaleAmount, _punchDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(_originalScale, _punchDuration);
    }
}

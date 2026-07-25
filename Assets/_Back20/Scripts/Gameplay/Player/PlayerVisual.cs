using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [Header("Death Effect Settings")]
    [SerializeField] private ParticleSystem _deathParticle;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _animationDuration = 0.5f;

    private void Awake()
    {
        if (_spriteRenderer == null)
            TryGetComponent(out _spriteRenderer);
    }

    public void PlayDeathEffect()
    {
        transform.DOKill();
        _deathParticle.Play();
        StartCoroutine(PlayDeathRoutine());

    }

    private IEnumerator PlayDeathRoutine()
    {
        _deathParticle.Play();
        transform.DOScale(Vector2.zero, _animationDuration);
        yield return new WaitForSeconds(_animationDuration);

        SceneController.Instance?.ReloadSceneWithTransition(false);
    }
}
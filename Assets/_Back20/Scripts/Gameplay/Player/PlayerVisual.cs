using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace MeowgaByte.Core
{
    public class PlayerVisual : MonoBehaviour
    {
        [Header("Death Effect Settings")]
        [SerializeField] private ParticleSystem _deathParticle;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;
        [SerializeField] private float _animationDuration = 0.5f;

        public readonly string ANIM_RUN = "animRun";
        public readonly string ANIM_JUMP = "animJump";
        public readonly string ANIM_FALL = "animFall";
        public readonly string ANIM_IDLE = "animIdle";

        private void Awake()
        {
            if (_spriteRenderer == null)
                TryGetComponent(out _spriteRenderer);
            if (_animator == null)
                TryGetComponent(out _animator);
        }

        private void Start()
        {
            UpdateAnimation(ANIM_IDLE);
        }

        public void PlayDeathEffect()
        {
            transform.DOKill();
            _deathParticle.Play();
            StartCoroutine(PlayDeathRoutine());

        }

        private string _currentAnim;

        public void UpdateAnimation(string anim)
        {
            if (_currentAnim == anim) return; 
            _currentAnim = anim;
            _animator.Play(anim);
        }

        private IEnumerator PlayDeathRoutine()
        {
            _deathParticle.Play();
            OverlayEffectController.Instance?.FlashScreen(OverlayEffectController.Instance.HazardColor, flashDuration: _animationDuration);
            OverlayEffectController.Instance?.FlashVignette(OverlayEffectController.Instance.HazardColor, flashDuration: _animationDuration);

            transform.DOScale(Vector2.zero, _animationDuration).SetEase(Ease.OutExpo);
            transform.DORotate(new Vector3(0, 0, 360), _animationDuration).SetEase(Ease.OutExpo);
            yield return new WaitForSeconds(_animationDuration);

            SceneController.Instance?.ReloadSceneWithTransition(false);
        }
    }
}
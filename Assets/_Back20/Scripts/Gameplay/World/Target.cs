using System.Collections;
using DG.Tweening;
using MeowgaByte.Core;
using MeowgaByte.Gameplay;
using UnityEngine;

namespace MeowgaByte.World
{
    public class Target : MonoBehaviour
    {
        [Header("Scene Settings")]
        [SerializeField] private string _nextLevelSceneName;
        [SerializeField] private float _delayBeforeTransition = 1.5f; 
        [SerializeField] private float _animationDuration = 0.7f;
        [SerializeField] private float _waitDuration = 0.5f;

        [SerializeField] private ParticleSystem _successParticles;

        private bool _isTriggered = false;

        [System.Obsolete]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isTriggered) return;

            if (collision.attachedRigidbody != null && 
                collision.attachedRigidbody.TryGetComponent<PlayerController>(out var player))
            {
                _isTriggered = true; 
                GameManager.Instance?.Win();
                StartCoroutine(WinSequence(player));
            }
        }

        [System.Obsolete]
        private IEnumerator WinSequence(PlayerController player)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Transform playerTransform = player.transform;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true; 
            }
            player.enabled = false; 


            if (_successParticles != null)
            {
                _successParticles.Play();
            }

            yield return new WaitForSeconds(_waitDuration);

            // Hút player vào giữa đích
            playerTransform.DOMove(this.transform.position, _animationDuration).SetEase(Ease.InBack);
            // Thu nhỏ player biến mất
            playerTransform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack);
            
            // Xoay vài vòng cho ảo diệu
            playerTransform.DORotate(new Vector3(0, 0, 360f), _animationDuration, RotateMode.FastBeyond360).SetEase(Ease.InBack);

            // Rung camera nhẹ 
            if (Camera.main != null)
            {
                Camera.main.transform.DOShakePosition(0.4f, 0.2f, 10, 90f);
            }

            yield return new WaitForSeconds(_delayBeforeTransition);
            if (_successParticles != null)
            {
                _successParticles.Play();
            }
            SceneController.Instance.LoadSceneWithName(_nextLevelSceneName); 
        }
    }    
}
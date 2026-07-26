using System.Collections;
using DG.Tweening;
using UnityEngine;
using MeowgaByte.Gameplay; 

public class MoveBlock : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _returnSpeed = 3f;
    [SerializeField] private float _delayTime = 0f;
    [SerializeField] private float _waitTime = 1f;
    [SerializeField] private Ease _moveEase = Ease.Linear;
    [SerializeField] private Ease _returnEase = Ease.Linear;
    [SerializeField] private Transform _target;
    [SerializeField] private ParticleSystem _particle;
    
    private Vector2 _originalPos;

    private void Start()
    {
        _originalPos = this.transform.position;
        
        if (_target != null)
        {
            StartCoroutine(Move());
        }
        else
        {
            Debug.LogWarning("Chưa gán Target cho MoveBlock nhé!");
        }
    }

    private IEnumerator Move()
    {
        yield return new WaitForSeconds(_delayTime);
        
        while (true)
        {
            float distanceToTarget = Vector2.Distance(transform.position, _target.position);
            float moveDuration = distanceToTarget / _moveSpeed;

            yield return transform.DOMove(_target.position, moveDuration)
                                  .SetEase(_moveEase)
                                  .WaitForCompletion();

            if (_particle != null)
                _particle.Play();
            if (_waitTime > 0) yield return new WaitForSeconds(_waitTime);

            float distanceToReturn = Vector2.Distance(transform.position, _originalPos);
            float returnDuration = distanceToReturn / _returnSpeed;

            yield return transform.DOMove(_originalPos, returnDuration)
                                  .SetEase(_returnEase)
                                  .WaitForCompletion();

            if (_waitTime > 0) yield return new WaitForSeconds(_waitTime);
        }
    }
    
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.TryGetComponent<PlayerController>(out _))
    //     {
    //         collision.transform.SetParent(this.transform);
    //     }
    // }

    // private void OnCollisionExit2D(Collision2D collision)
    // {
    //     if (collision.gameObject.TryGetComponent<PlayerController>(out _))
    //     {
    //         collision.transform.SetParent(null);
    //     }
    // }
}
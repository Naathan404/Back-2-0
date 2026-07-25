using UnityEngine;
using MeowgaByte.Gameplay; // Thay đổi theo namespace chứa PlayerController của ông

namespace MeowgaByte.World
{
    public enum SignDirection
    {
        Left,
        Right
    }

    public class Sign : MonoBehaviour
    {
        [Header("Sign Settings")]
        [Tooltip("Hướng mà biển báo sẽ ép người chơi đi theo")]
        [SerializeField] private SignDirection _direction = SignDirection.Right;

        [SerializeField] private ParticleSystem _particle;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null && 
                collision.attachedRigidbody.TryGetComponent<PlayerController>(out var player))
            {
                _particle.Play();
                player.HandleSignDirection(_direction);
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = _direction == SignDirection.Right ? Color.cyan : Color.magenta;
            Vector3 pointDir = _direction == SignDirection.Right ? Vector3.right : Vector3.left;
            
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + pointDir * 1.5f;

            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawSphere(endPos, 0.2f);
        }
    }
}
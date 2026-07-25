using UnityEngine;
using MeowgaByte.Gameplay;
using Unity.Cinemachine; 

namespace MeowgaByte.World
{
    public class ConfinerTrigger : MonoBehaviour
    {
        [Header("Cinemachine Settings")]
        [SerializeField] private CinemachineConfiner2D _cameraConfiner;
        
        [SerializeField] private Collider2D _newConfinerBounds;

        private bool _isSwitched = false;

        [System.Obsolete]
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_isSwitched) return;

            if (collision.attachedRigidbody != null && 
                collision.attachedRigidbody.TryGetComponent<PlayerController>(out _))
            {
                SwitchConfiner();
            }
        }

        [System.Obsolete]
        private void SwitchConfiner()
        {
            if (_cameraConfiner != null && _newConfinerBounds != null)
            {
                _cameraConfiner.BoundingShape2D = _newConfinerBounds;

                _cameraConfiner.InvalidateCache();
                
                _isSwitched = true;
            }
            else
            {
                Debug.LogWarning("Chưa gán Confiner hoặc Bounds mới vào script nhé!");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null && 
                collision.attachedRigidbody.TryGetComponent<PlayerController>(out _))
            {
                _isSwitched = false; 
            }
        }
    }
}
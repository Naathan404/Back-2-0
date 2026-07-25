using MeowgaByte.Gameplay;
using UnityEngine;

namespace MeowgaByte.World
{
    public class Target : MonoBehaviour
    {
        [SerializeField] private string _nextLevelSceneName;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<PlayerController>(out _))
            {
                SceneController.Instance.LoadSceneWithName(_nextLevelSceneName);
            }
        }
    }    
}


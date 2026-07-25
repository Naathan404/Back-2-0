using UnityEngine;

namespace MeowgaByte.Data
{
    [CreateAssetMenu(fileName = "PlayerDataConfig", menuName = "MeowgaByte/Player Data")]
    public class PlayerData : ScriptableObject
    {
        
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private float _jumpForce = 4f;

        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
    }
}
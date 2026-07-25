using MeowgaByte.Core;
using MeowgaByte.Data;
using MeowgaByte.World;
using UnityEngine;

namespace MeowgaByte.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Refereces")]
        [SerializeField] private PlayerData _playerData;
        [SerializeField] private PlayerVisual _visual;
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private InfoPanelController _info;

        [Header("Debug field")]
        [SerializeField] private bool _isMoving = false;
        [SerializeField] private float _moveDirection = 1;
        [SerializeField] private bool _enableDebugInput = true;

        private void Start()
        {
            if (_playerData == null)
            {
                DebugHandler.LogError(this.name, "Missing Player Data Config");
                return;
            }
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
            }
        }

        private void FixedUpdate()
        {
            if (_isMoving)
            {
                _rb.linearVelocity = new Vector2(_moveDirection * _playerData.MoveSpeed, _rb.linearVelocity.y);
            }
        }

        #region Execute Action
        /// <summary>
        /// Hàm set hướng di chuyển cho nhân vật <Duration>
        /// </summary>
        /// <param name="direction" > 0 di chuyển phải, < 0 di chuyển trái></param>
        public void SetMoveDirection(int direction)
        {
            _isMoving = true;
            _moveDirection = direction;
            this.transform.localScale = new Vector2(direction, 1);
        }

        /// <summary>
        /// Hàm gọi nhân vật đứng đợi trong 
        /// </summary>
        public void StopMovement()
        {
            _isMoving = false;
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
        }
        
        /// <summary>
        /// Hàm nhảy
        /// </summary>
        public bool TryJump()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _playerData.JumpForce);
            return true;
        }

        /// <summary>
        /// Trả về điểm spawn
        /// </summary>
        public void Respawn()
        {
            if (_spawnPoint == null)
            {
                DebugHandler.LogError(this.name, "Missing Spawnpoint");
                return;
            }
            this.transform.position = _spawnPoint.position;
            _isMoving = false;
            _moveDirection = 1;
            _rb.linearVelocity = Vector2.zero;
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.TryGetComponent<IHarmful>(out _))
            {
                GameManager.Instance?.Lose();
            }
        }

        /// <summary>
        /// Hàm này được gọi bởi object Sign khi người chơi đi ngang qua
        /// </summary>
        public void HandleSignDirection(SignDirection newDirection)
        {
            if (_isMoving) 
            {
                if (newDirection == SignDirection.Left)
                {
                    SetMoveDirection(-1);
                }
                else if (newDirection == SignDirection.Right)
                {
                    SetMoveDirection(1);
                }
                _info.UpdateNotifyText("Change Direction!");
            }
        }

        /// <summary>
        /// Gọi hàm này cho Player chết
        /// </summary>
        public void Die()
        {
            _visual.PlayDeathEffect();
        }

        #endregion
    }
}

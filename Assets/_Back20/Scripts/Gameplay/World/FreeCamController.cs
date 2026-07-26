using UnityEngine;
using Unity.Cinemachine;
using DG.Tweening; // Thư viện của Cinemachine mới

namespace MeowgaByte.Gameplay
{
    public class FreeCameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [Tooltip("Object CinemachineCamera bám theo người chơi")]
        [SerializeField] private CinemachineCamera _playerCam;
        [Tooltip("Object CinemachineCamera dùng để nhìn tự do")]
        [SerializeField] private CinemachineCamera _freeCam;

        [SerializeField] private GameObject _freeCamPanel;
        [SerializeField] private Transform _aButton;
        [SerializeField] private Transform _dButton;
        [SerializeField] private Transform _wButton;
        [SerializeField] private Transform _sButton;

        [Header("Movement Settings")]
        [SerializeField] private float _panSpeed = 15f;
        [SerializeField] private float _dragSpeed = 2f; 

        private bool _isFreeCamActive = true; 
        private Vector3 _dragOrigin;

        private void Start()
        {
            ActivateFreeCam();
        }

        private void Update()
        {
            if (!_isFreeCamActive) return;

            HandleKeyboardMovement();
            HandleMouseDragMovement();
        }

        private void HandleKeyboardMovement()
        {
            float x = Input.GetAxisRaw("Horizontal"); 
            float y = Input.GetAxisRaw("Vertical");   
            _aButton.transform.localScale = Vector2.one;
            _dButton.transform.localScale = Vector2.one;
            _wButton.transform.localScale = Vector2.one;
            _sButton.transform.localScale = Vector2.one;

            if (x != 0 || y != 0)
            {
                if (x > 0)
                {
                    _dButton.transform.localScale = 1.25f * Vector2.one;
                }
                if (x < 0)
                {
                    _aButton.transform.localScale = 1.25f * Vector2.one;
                }
                if (y > 0)
                {
                    _sButton.transform.localScale = 1.25f * Vector2.one;
                }
                if (y < 0)
                {
                    _wButton.transform.localScale = 1.25f * Vector2.one;
                }
                Vector3 moveDir = new Vector3(x, y, 0).normalized;
                transform.position += moveDir * _panSpeed * Time.deltaTime;
            }
        }

        private void HandleMouseDragMovement()
        {
            if (Input.GetMouseButtonDown(2) || Input.GetMouseButtonDown(1))
            {
                _dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
            {
                Vector3 difference = _dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                transform.position += difference * _dragSpeed; 
            }
        }


        public void ActivateFreeCam()
        {
            _isFreeCamActive = true;

            _freeCam.gameObject.SetActive(true);
            _freeCamPanel.SetActive(true);
            _playerCam.gameObject.SetActive(false);
            Vector3 playerPos = _playerCam.transform.position;
            transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
        }

        public void ActivatePlayerCam()
        {
            _isFreeCamActive = false;

            _playerCam.gameObject.SetActive(true);
            _freeCam.gameObject.SetActive(false);
            _freeCamPanel.SetActive(false);
        }
    }
}
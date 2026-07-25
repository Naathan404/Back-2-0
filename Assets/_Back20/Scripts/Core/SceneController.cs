using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SceneController : MonoSingleton<SceneController>
{
    [Header("Digital Transition Settings")]
    [SerializeField] private Color _transitionColor = new Color(0.1f, 0.1f, 0.15f, 1f); // Màu tối kiểu Lab
    [SerializeField] private float _animationDuration = 0.4f;
    [SerializeField] private int _sliceCount = 10; // Số lượng thanh cắt ngang
    private List<RectTransform> _slices = new List<RectTransform>();

    [Header("Mosaic Transition Settings")]
    [SerializeField] private Color _mosaicColor = new Color(0.15f, 0.7f, 0.8f, 1f); // Xanh lam Neon
    [SerializeField] private float _mosaicAnimDuration = 0.3f;
    [SerializeField] private int _mosaicRows = 8; // Số hàng ngang của lưới
    private List<RectTransform> _mosaicTiles = new List<RectTransform>();
    private Transform _mosaicContainer;


    private Canvas _transitionCanvas;
    private GraphicRaycaster _raycaster;
    private bool _isTransitioning = false;

    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
        SetupTransitionUI();
        SetupMosaicUI();
    }

    /// <summary>
    /// Tự động tạo UI Canvas và các thanh sọc ngay bằng Code
    /// </summary>
    private void SetupTransitionUI()
    {
        // Tạo Canvas tĩnh nằm trên mọi thứ (Ép kiểu RectTransform ngay từ đầu)
        GameObject canvasObj = new GameObject("DigitalTransitionCanvas", typeof(RectTransform));
        canvasObj.transform.SetParent(this.transform, false); // false để giữ tọa độ local chuẩn
        
        _transitionCanvas = canvasObj.AddComponent<Canvas>();
        _transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _transitionCanvas.sortingOrder = 999; 

        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        _raycaster.enabled = false; 

        // Cắt màn hình thành các thanh ngang
        float sliceHeight = 1080f / _sliceCount; 
        
        for (int i = 0; i < _sliceCount; i++)
        {
            // Ép kiểu RectTransform ngay lúc sinh ra
            GameObject sliceObj = new GameObject($"Slice_{i}", typeof(RectTransform));
            sliceObj.transform.SetParent(canvasObj.transform, false);
            
            Image img = sliceObj.AddComponent<Image>();
            img.color = _transitionColor;

            RectTransform rect = sliceObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            
            rect.anchoredPosition = new Vector2(0, -i * sliceHeight);
            rect.sizeDelta = new Vector2(0, sliceHeight); 
            rect.localScale = new Vector3(0, 1, 1); 
            
            _slices.Add(rect);
        }
    }

    /// <summary>
    /// Khởi tạo các khối vuông Mosaic bằng code
    /// </summary>
    private void SetupMosaicUI()
    {
        // Ép kiểu RectTransform ngay lập tức
        GameObject containerObj = new GameObject("MosaicContainer", typeof(RectTransform));
        containerObj.transform.SetParent(_transitionCanvas.transform, false);
        _mosaicContainer = containerObj.transform;

        // Lấy RectTransform ra dùng, KHÔNG dùng AddComponent nữa
        RectTransform containerRect = containerObj.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        float referenceHeight = 1080f; 
        float tileSize = referenceHeight / _mosaicRows;
        int cols = Mathf.CeilToInt(1920f / tileSize) + 1;

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < _mosaicRows; y++)
            {
                // Khởi tạo các tile con cũng phải là RectTransform trước khi gán Parent
                GameObject tileObj = new GameObject($"Tile_{x}_{y}", typeof(RectTransform));
                tileObj.transform.SetParent(_mosaicContainer, false);
                
                Image img = tileObj.AddComponent<Image>();
                img.color = _mosaicColor;

                RectTransform rect = tileObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                
                rect.anchoredPosition = new Vector2(x * tileSize + (tileSize / 2f), y * tileSize + (tileSize / 2f));
                rect.sizeDelta = new Vector2(tileSize + 2f, tileSize + 2f); 
                rect.localScale = Vector3.zero; 
                
                _mosaicTiles.Add(rect);
            }
        }
    }

    public void ReloadScene()
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionRoutine(SceneManager.GetActiveScene().name));
    }

    public void ReloadSceneWithTransition(bool random = true)
    {
        if (_isTransitioning) return;
        if (random)
        {
            float rand = Random.Range(0f, 1f);
            if (rand > 0.5f)
            {
                StartCoroutine(TransitionRoutine(SceneManager.GetActiveScene().name));
            }
            else
            {
                StartCoroutine(TransitionMosaicRoutine(SceneManager.GetActiveScene().name));
            }
        }
        else
        {
            StartCoroutine(TransitionRoutine(SceneManager.GetActiveScene().name));
        }
    }

    public void LoadSceneWithName(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionMosaicRoutine(sceneName));
    }

    private IEnumerator TransitionRoutine(string sceneName)
    {
        _isTransitioning = true;
        _raycaster.enabled = true; 

        // wipe in
        Sequence wipeIn = DOTween.Sequence();
        for (int i = 0; i < _slices.Count; i++)
        {
            // Các thanh chẵn phi từ Trái sang, lẻ phi từ Phải sang
            _slices[i].pivot = (i % 2 == 0) ? new Vector2(0, 1) : new Vector2(1, 1);
        
            wipeIn.Insert(i * 0.05f, _slices[i].DOScaleX(1f, _animationDuration).SetEase(Ease.OutQuint));
        }

        // Đợi DOTween chạy xong
        yield return wipeIn.WaitForCompletion();

        // load scene
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // wipe ouut
        Sequence wipeOut = DOTween.Sequence();
        for (int i = 0; i < _slices.Count; i++)
        {
            // Đảo Pivot để thanh tiếp tục lướt sang bờ bên kia thay vì giật ngược lại
            _slices[i].pivot = (i % 2 == 0) ? new Vector2(1, 1) : new Vector2(0, 1);
            
            wipeOut.Insert(i * 0.05f, _slices[i].DOScaleX(0f, _animationDuration).SetEase(Ease.InQuint));
        }

        yield return wipeOut.WaitForCompletion();

        _raycaster.enabled = false;
        _isTransitioning = false;
    }

    public void LoadSceneMosaicWipe(string sceneName)
    {
        if (_isTransitioning) return;
        StartCoroutine(TransitionMosaicRoutine(sceneName));
    }

    private IEnumerator TransitionMosaicRoutine(string sceneName)
    {
        _isTransitioning = true;
        _raycaster.enabled = true;
        _mosaicContainer.SetAsLastSibling(); 
        // Xáo trộn ngẫu nhiên danh sách để các khối hiện ra lộn xộn
        // Nếu không xáo trộn, nó sẽ quét chéo từ góc màn hình lên rất đẹp
        System.Random rng = new System.Random();
        var shuffledTiles = new List<RectTransform>(_mosaicTiles);
        int n = shuffledTiles.Count;
        while (n > 1) 
        {
            n--;
            int k = rng.Next(n + 1);
            var value = shuffledTiles[k];
            shuffledTiles[k] = shuffledTiles[n];
            shuffledTiles[n] = value;
        }

        // wipe in
        Sequence wipeIn = DOTween.Sequence();
        for (int i = 0; i < shuffledTiles.Count; i++)
        {
            // Delay mỗi ô cách nhau một chút xíu (0.005s) tạo cảm giác dồn dập
            wipeIn.Insert(i * 0.005f, shuffledTiles[i].DOScale(1f, _mosaicAnimDuration).SetEase(Ease.OutBack));
        }

        yield return wipeIn.WaitForCompletion();

        // load scene
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // wipe out 
        Sequence wipeOut = DOTween.Sequence();
        for (int i = 0; i < shuffledTiles.Count; i++)
        {
            // Rút lại cũng theo thứ tự ngẫu nhiên
            wipeOut.Insert(i * 0.005f, shuffledTiles[i].DOScale(0f, _mosaicAnimDuration).SetEase(Ease.InBack));
        }

        yield return wipeOut.WaitForCompletion();

        _raycaster.enabled = false;
        _isTransitioning = false;
    }
}
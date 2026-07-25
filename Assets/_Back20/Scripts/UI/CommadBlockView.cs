using UnityEngine;
using UnityEngine.UI;

namespace MeowgaByte.UI
{
    public class CommandBlockView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;

        public void SetIcon(Sprite icon, float x, float y)
        {
            if (_iconImage == null || icon == null) return;
            _iconImage.sprite = icon;
            _iconImage.rectTransform.sizeDelta = new Vector2(x, y);
        }
    }
}
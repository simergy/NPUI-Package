using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace NP_UI
{
    /// <summary>
    /// A modular component that can be attached to any UI handle to resize a target RectTransform.
    /// </summary>
    public class NP_MenuResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // --- Configuration ---
        private RectTransform _target;
        private ResizeSettings _settings;
        private Action<Vector2> _onResizeCallback;

        // --- State ---
        private Vector2 _initialSize;
        private Vector2 _initialPointerPos;
        private Vector2 _minSizePixels;
        private Vector2 _maxSizePixels;
        private bool _isDragging;

        // --- Visuals ---
        private Texture2D _cursor;
        private Vector2 _cursorHotspot = new Vector2(16, 16);

        /// <summary>
        /// Modular setup for the resizer.
        /// </summary>
        /// <param name="target">The RectTransform to be resized.</param>
        /// <param name="settings">Constraint and permission settings.</param>
        /// <param name="onResize">Optional callback to sync other components (like Grids).</param>
        public void Setup(RectTransform target, ResizeSettings settings, Action<Vector2> onResize = null)
        {
            _target = target;
            _settings = settings;
            _onResizeCallback = onResize;
            _cursor = Resources.Load<Texture2D>("Small Icons/WhiteScaleIcon");
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_target == null) return;

            _isDragging = true;
            _initialSize = _target.sizeDelta;

            CalculatePixelConstraints();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _target.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out _initialPointerPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _target == null) return;

            // Step 1: Get standardized input
            RectTransform parentRect = _target.parent as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, 
                eventData.position, 
                eventData.pressEventCamera, 
                out Vector2 currentPointerPos);

            // Step 2: Calculate the logical new size
            Vector2 newSize = CalculateNewSize(currentPointerPos);

            // Step 3: Apply the size to the UI
            _target.sizeDelta = newSize;

            // Step 4: Modular notification (The "Observer" pattern)
            // We notify any external systems (like Grids or Accordions) that a resize happened.
            _onResizeCallback?.Invoke(newSize);

            // Step 5: Force visual refresh
            LayoutRebuilder.ForceRebuildLayoutImmediate(_target);
        }
        
        private Vector2 CalculateNewSize(Vector2 currentPointerPos)
        {
            Vector2 pointerDelta = currentPointerPos - _initialPointerPos;
            Vector2 calculatedSize = _initialSize;

            // Pivot multipliers (doubles delta if pivot is centered)
            float multX = (_target.pivot.x == 0.5f) ? 2.0f : 1.0f;
            float multY = (_target.pivot.y == 0.5f) ? 2.0f : 1.0f;

            // Horizontal Resizing
            if (_settings.CanResizeRight) 
                calculatedSize.x = _initialSize.x + (pointerDelta.x * multX);
            else if (_settings.CanResizeLeft) 
                calculatedSize.x = _initialSize.x - (pointerDelta.x * multX); // Inverted for left side

            // Vertical Resizing
            if (_settings.CanResizeBottom) 
                calculatedSize.y = _initialSize.y - (pointerDelta.y * multY);
            else if (_settings.CanResizeTop) 
                calculatedSize.y = _initialSize.y + (pointerDelta.y * multY); // Inverted for top side

            // Apply pixel-based constraints
            calculatedSize.x = Mathf.Clamp(calculatedSize.x, _minSizePixels.x, _maxSizePixels.x);
            calculatedSize.y = Mathf.Clamp(calculatedSize.y, _minSizePixels.y, _maxSizePixels.y);

            return calculatedSize;
        }

        /// <summary>
        /// Calculate pixels ration to canvas size
        /// </summary>
        private void CalculatePixelConstraints()
        {
            Canvas canvas = _target.GetComponentInParent<Canvas>();
            float scale = (canvas != null) ? canvas.scaleFactor : 1.0f;

            _minSizePixels = new Vector2(
                (Screen.width * _settings.MinPercent.x) / scale,
                (Screen.height * _settings.MinPercent.y) / scale);

            _maxSizePixels = new Vector2(
                (Screen.width * _settings.MaxPercent.x) / scale,
                (Screen.height * _settings.MaxPercent.y) / scale);
        }

        // --- Event Implementation ---
        public void OnEndDrag(PointerEventData eventData) 
        { 
            _isDragging = false; 
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        
        public void OnPointerEnter(PointerEventData eventData) => Cursor.SetCursor(_cursor, _cursorHotspot, CursorMode.Auto);
        public void OnPointerExit(PointerEventData eventData) { if (!_isDragging) Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); }
        private void OnDisable() => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    [Serializable]
    public struct ResizeSettings
    {
        public bool CanResizeLeft, CanResizeRight;
        public bool CanResizeTop, CanResizeBottom;
        public Vector2 MinPercent;
        public Vector2 MaxPercent;
    }
}
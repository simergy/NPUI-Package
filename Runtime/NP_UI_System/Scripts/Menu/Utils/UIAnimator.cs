using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
    /// Handles movement and transformation animations for a UI element (RectTransform).
    /// This component can be attached to any panel, menu, or button.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIAnimator : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Vector2 startPosition;
        private void Start()
        {
            startPosition = GetComponent<RectTransform>().anchoredPosition;
        }
        
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("UIAnimator requires a RectTransform component on the same GameObject.");
            }
        }

        /// <summary>
        /// Moves the UI element smoothly to a target position.
        /// </summary>
        /// <param name="targetPosition">The desired local position.</param>
        /// <param name="duration">How long the movement should take (in seconds).</param>
        public void MoveTo(Vector2 targetPosition, float duration = 0.3f)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateMovement(targetPosition, duration));
        }

        public void MoveToStart(float duration = 0.3f)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateMovement(startPosition, duration));
        }

        /// <summary>
        /// Coroutine to handle the smooth, frame-rate independent movement.
        /// </summary>
        private IEnumerator AnimateMovement(Vector2 targetPosition, float duration)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            float startTime = Time.time;
            float endTime = startTime + duration;

            while (Time.time < endTime)
            {
                float t = (Time.time - startTime) / duration;
                // Optional: Use Easing functions like SmoothStep for better feel
                t = t * t * (3f - 2f * t); 
                
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null; // Wait for the next frame
            }

            // Ensure the final position is exactly the target
            rectTransform.anchoredPosition = targetPosition;
        }

        // Add other animation methods here (e.g., FadeOut, ScaleUp)
    }
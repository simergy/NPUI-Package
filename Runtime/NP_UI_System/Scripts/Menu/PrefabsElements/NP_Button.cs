using System;
using DA_Assets.DAG;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NP_UI
{
// --- NP_Button ---
    /// <summary>
    /// Represents a UI Button element, capable of being clicked and having an image.
    /// Implements IClickableElement and IImageableElement interfaces.
    /// </summary>
    public class NP_Button : NP_UIElements, IClickableElement, IImageableElement, ITextableElement
    {
        [SerializeField] protected Button buttonComponent; // Reference to Unity's Button component
        [SerializeField] protected Image backgroundImageComponent; // Reference to Unity's RawImage component
        [SerializeField] protected DAGradient dagBackgroundImageComponent; // Reference to Unity's RawImage component
        [SerializeField] protected TextMeshProUGUI textHeadLine; // Reference to Unity's RawImage component

        protected override void Awake()
        {
            base.Awake();
            buttonComponent = GetComponent<Button>();
            uiRectTransform.sizeDelta = Size;
            if (buttonComponent == null)
            {
                Debug.LogError("NP_Button requires a Button component on its GameObject.", this);
            }

            if (backgroundImageComponent == null)
            {
                Debug.LogWarning("NP_Button: No RawImage component found to set background image.", this);
            }

            if (dagBackgroundImageComponent == null)
            {
                Debug.LogWarning("NP_Button: No RawImage component found to set background image.", this);
            }
        }

        public void SetOnClick(UnityAction onClickAction)
        {
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners(); // Clear previous listeners
                buttonComponent.onClick.AddListener(onClickAction);
            }
        }
        
        public void AddOnClick(UnityAction onClickAction)
        {
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(onClickAction);
            }
        }

        public void SetBackgroundImage(Sprite texture)
        {
            if (backgroundImageComponent != null && texture != null)
            {
                buttonComponent.image.sprite = texture;
            }
            else if (texture == null)
            {
                Debug.LogWarning("NP_Button: Provided texture is null for SetBackgroundImage.", this);
            }
        }


        public void SetText(string text)
        {
            if (textHeadLine != null)
            {
                textHeadLine.text = text;
            }
        }

        public void SetBackgroundColor(Color color)
        {
            ColorChanger.ShiftGradientHueDAG(dagBackgroundImageComponent, color);
            //ColorChanger.DarkenExistingGradientDAG(dagBackgroundImageComponent, 1f);
        }

        public void SetTextColor(Color color)
        {
            throw new NotImplementedException();
        }

        public void SetBold(bool isBold)
        {
            throw new NotImplementedException();
        }
    }
}
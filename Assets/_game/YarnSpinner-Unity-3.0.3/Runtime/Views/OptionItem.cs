/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity.Attributes;

#if USE_TMP
using TMPro;
#else
    using TextMeshProUGUI = Yarn.Unity.TMPShim;
#endif

#nullable enable

namespace Yarn.Unity
{
    [System.Serializable]
    internal struct InternalAppearance
    {
        [SerializeField] internal Sprite sprite;
        [SerializeField] internal Color colour;
    }

    public sealed class OptionItem : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler
    {
        [MustNotBeNull, SerializeField] TextMeshProUGUI? text;
        [SerializeField] UnityEngine.UI.Image? selectionImage;

        [Group("Appearance"), SerializeField] InternalAppearance normal;
        [Group("Appearance"), SerializeField] InternalAppearance selected;
        [Group("Appearance"), SerializeField] InternalAppearance disabled;

        // --- NEW: Alpha Coach Styling ---
        [Group("Alpha Appearance"), SerializeField] InternalAppearance alphaNormal;
        [Group("Alpha Appearance"), SerializeField] InternalAppearance alphaSelected;
        [Group("Alpha Appearance"), SerializeField, Tooltip("Optional: A child GameObject containing a neon border or glitch effect that only turns on for Alpha options.")]
        GameObject? alphaBorderOverlay;

        private bool isAlphaStyle = false;
        // --------------------------------

        [Group("Appearance"), SerializeField] bool disabledStrikeThrough = true;

        public YarnTaskCompletionSource<DialogueOption?>? OnOptionSelected;
        public System.Threading.CancellationToken completionToken;

        private bool hasSubmittedOptionSelection = false;

        private DialogueOption? _option;
        public DialogueOption Option
        {
            get
            {
                if (_option == null)
                {
                    throw new System.NullReferenceException("Option has not been set on the option item");
                }
                return _option;
            }

            set
            {
                _option = value;

                hasSubmittedOptionSelection = false;

                // --- NEW: Check Yarn 3.0 Metadata for the Alpha tag ---
                isAlphaStyle = false;
                if (value.Line.Metadata != null)
                {
                    foreach (var tag in value.Line.Metadata)
                    {
                        // StringComparison.OrdinalIgnoreCase makes it safe even if you type #alphaoption or #AlphaOption
                        if (tag.Trim().Equals("AlphaOption", System.StringComparison.OrdinalIgnoreCase))
                        {
                            isAlphaStyle = true;
                            break;
                        }
                    }
                }

                // Toggle the custom border graphic if you assigned one in the inspector
                if (alphaBorderOverlay != null)
                {
                    alphaBorderOverlay.SetActive(isAlphaStyle);
                }
                // ------------------------------------------------------

                // When we're given an Option, use its text and update our
                // interactibility.
                string line = value.Line.TextWithoutCharacterName.Text;
                if (disabledStrikeThrough && !value.IsAvailable)
                {
                    line = $"<s>{value.Line.TextWithoutCharacterName.Text}</s>";
                }

                if (text == null)
                {
                    Debug.LogWarning($"The {nameof(text)} is null, is it not connected in the inspector?", this);
                    return;
                }

                text.text = line;
                interactable = value.IsAvailable;

                // Apply the correct styling (Alpha vs Normal)
                ApplyStyle(isAlphaStyle ? alphaNormal : normal);
            }
        }

        private void ApplyStyle(InternalAppearance style)
        {
            Color newColour = style.colour;
            Sprite newSprite = style.sprite;
            if (!Option.IsAvailable)
            {
                newColour = disabled.colour;
                newSprite = disabled.sprite;
            }

            if (text == null)
            {
                Debug.LogWarning($"The {nameof(text)} is null, is it not connected in the inspector?", this);
                return;
            }

            text.color = newColour;

            if (selectionImage != null)
            {
                selectionImage.color = newColour;
                if (newSprite != null)
                {
                    selectionImage.sprite = newSprite;
                    selectionImage.gameObject.SetActive(true);
                }
                else
                {
                    selectionImage.gameObject.SetActive(false);
                }
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            // Use alpha styling if tagged!
            ApplyStyle(isAlphaStyle ? alphaSelected : selected);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            // Use alpha styling if tagged!
            ApplyStyle(isAlphaStyle ? alphaNormal : normal);
        }

        new public bool IsHighlighted
        {
            get
            {
                return EventSystem.current.currentSelectedGameObject == this.gameObject;
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            if (!IsInteractable())
            {
                return;
            }

            if (hasSubmittedOptionSelection == false && !completionToken.IsCancellationRequested)
            {
                hasSubmittedOptionSelection = true;
                OnOptionSelected?.TrySetResult(this.Option);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InvokeOptionSelected();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
        }
    }
}

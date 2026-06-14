using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Yarn.Unity
{
    public class DialogueBoxSwapper : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Drag your Background Image component here.")]
        [SerializeField] private Image backgroundTarget = null!;

        [Tooltip("Drag the TextMeshProUGUI component used for the Dialogue Text here.")]
        [SerializeField] private TextMeshProUGUI dialogueTextTarget = null!;

        [Tooltip("Drag the TextMeshProUGUI component used for the Character Name here. (Optional)")]
        [SerializeField] private TextMeshProUGUI nameTextTarget = null!;

        [Header("Character Looks")]
        [SerializeField] private DialogLook claudeLook;
        [SerializeField] private DialogLook maryLook;
        [SerializeField] private DialogLook alphaCoachLook;
        [SerializeField] private DialogLook narrationLook; // For no speaker

        // --- NEW: Cache the default settings ---
        private TMP_FontAsset defaultDialogueFont = null!;
        private Color defaultDialogueColor;

        private TMP_FontAsset defaultNameFont = null!;
        private Color defaultNameColor;

        private void Awake()
        {
            // Memorize the original fonts and colors when the scene starts
            if (dialogueTextTarget != null)
            {
                defaultDialogueFont = dialogueTextTarget.font;
                defaultDialogueColor = dialogueTextTarget.color;
            }

            if (nameTextTarget != null)
            {
                defaultNameFont = nameTextTarget.font;
                defaultNameColor = nameTextTarget.color;
            }
        }

        public void SwapAppearance(string? characterName)
        {
            if (backgroundTarget == null)
            {
                Debug.LogWarning("DialogueBoxSwapper: No background Image assigned!");
                return;
            }

            if (string.IsNullOrWhiteSpace(characterName))
            {
                ApplyLook(narrationLook);
                return;
            }

            string nameLower = characterName.Trim().ToLower();

            switch (nameLower)
            {
                case "claude":
                    ApplyLook(claudeLook);
                    break;
                case "mary":
                    ApplyLook(maryLook);
                    break;
                case "alpha coach":
                    ApplyLook(alphaCoachLook);
                    break;
                default:
                    ApplyLook(narrationLook);
                    break;
            }
        }

        private void ApplyLook(DialogLook look)
        {
            // 1. Swap the Background Sprite
            if (look.boxSprite != null)
            {
                backgroundTarget.sprite = look.boxSprite;
            }

            // 2. Apply Dialogue Text Settings
            if (dialogueTextTarget != null)
            {
                dialogueTextTarget.font = look.usesUniqueFont && look.textFont != null
                    ? look.textFont
                    : defaultDialogueFont;

                dialogueTextTarget.color = look.usesUniqueTextColor
                    ? look.textColor
                    : defaultDialogueColor;
            }

            // 3. Apply Name Text Settings
            if (nameTextTarget != null)
            {
                nameTextTarget.font = look.usesUniqueFont && look.textFont != null
                    ? look.textFont
                    : defaultNameFont;

                nameTextTarget.color = look.usesUniqueTextColor
                    ? look.textColor
                    : defaultNameColor;
            }
        }
    }

    // Moved to the bottom so Unity reads the MonoBehaviour first!
    [System.Serializable]
    public struct DialogLook
    {
        public Sprite boxSprite;

        [Header("Font Settings")]
        public bool usesUniqueFont;
        public TMP_FontAsset textFont;

        [Header("Color Settings")]
        public bool usesUniqueTextColor;
        public Color textColor;
    }
}

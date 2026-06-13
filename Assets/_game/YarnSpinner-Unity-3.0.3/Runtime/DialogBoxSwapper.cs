using UnityEngine;
using UnityEngine.UI;

namespace Yarn.Unity
{
    public class DialogueBoxSwapper : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("Drag your Background Image component here.")]
        [SerializeField] private Image backgroundTarget = null!;

        [Header("Character Sprites")]
        [SerializeField] private Sprite claudeBox = null!;
        [SerializeField] private Sprite maryBox = null!;
        [SerializeField] private Sprite alphaCoachBox = null!;
        [SerializeField] private Sprite narrationBox = null!; // For no speaker

        /// <summary>
        /// Takes the character name directly from Yarn and assigns the correct sprite.
        /// </summary>
        public void SwapSprite(string? characterName)
        {
            if (backgroundTarget == null)
            {
                Debug.LogWarning("DialogueBoxSwapper: No background Image assigned!");
                return;
            }

            // If there's no name, default to the narration box
            if (string.IsNullOrWhiteSpace(characterName))
            {
                backgroundTarget.sprite = narrationBox;
                return;
            }

            // Clean up the string to avoid case-sensitivity or accidental space issues
            string nameLower = characterName.Trim().ToLower();

            switch (nameLower)
            {
                case "claude":
                    backgroundTarget.sprite = claudeBox;
                    break;
                case "mary":
                    backgroundTarget.sprite = maryBox;
                    break;
                case "alpha coach":
                    backgroundTarget.sprite = alphaCoachBox;
                    break;
                default:
                    // Failsafe for any unexpected characters
                    backgroundTarget.sprite = narrationBox;
                    break;
            }
        }
    }
}
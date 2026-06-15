using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using System.Threading;
using System.Collections.Generic;
using TMPro;

public class TypewriterSoundHandler : ActionMarkupHandler
{
    [Header("Audio Settings")]
    [SerializeField] private float minPitch = 0.80f;
    [SerializeField] private float maxPitch = 1.20f;
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    [Header("Interrupt Settings")]
    [Tooltip("If checked, the previous character's sound is cut off before the next character plays. If unchecked, the sounds will overlap.")]
    [SerializeField] private bool stopAudioSource = false;

    [Header("Anti-Spam Settings")]
    [Tooltip("Minimum time (in seconds) that must pass between sounds to prevent burst noises when skipping.")]
    [SerializeField] private float soundCooldown = 0.02f;
    private float lastSoundTime;

    [Header("UI References")]
    [Tooltip("Drag the TextMeshProUGUI component that displays the character name here.")]
    [SerializeField] private TMP_Text nameTextComponent;

    [Header("Character Sound Mapping")]
    [SerializeField] private List<CharacterSoundMap> characterSounds = new List<CharacterSoundMap>();
    [SerializeField] private string defaultSound = "TypewriterClick";

    [System.Serializable]
    public struct CharacterSoundMap
    {
        public string characterName;
        public string soundKey;
    }

    private AudioSource managedAudioSource;

    private void Awake()
    {
        managedAudioSource = GetComponent<AudioSource>();
        if (managedAudioSource == null)
        {
            managedAudioSource = gameObject.AddComponent<AudioSource>();
            managedAudioSource.playOnAwake = false;
        }
    }

    public override void OnLineDisplayBegin(MarkupParseResult line, TMP_Text text)
    {
        lastSoundTime = 0f;
    }

    public override void OnPrepareForLine(MarkupParseResult line, TMP_Text text)
    {
        // No setup needed
    }

    public override YarnTask OnCharacterWillAppear(int index, MarkupParseResult line, CancellationToken cancellationToken)
    {

        if (index < 0 || index >= line.Text.Length)
        {
            return YarnTask.CompletedTask;
        }

        if (Time.time - lastSoundTime < soundCooldown)
        {
            return YarnTask.CompletedTask;
        }

        char currentChar = line.Text[index];
        if (char.IsWhiteSpace(currentChar))
        {
            return YarnTask.CompletedTask;
        }

        string soundToPlay = defaultSound;

        if (!line.Text.Contains("[Fo]") && !line.Text.Contains("[In]") && !line.Text.Contains("[Em]"))
        {
            if (nameTextComponent != null && !string.IsNullOrEmpty(nameTextComponent.text))
            {
                string speakerName = nameTextComponent.text.Trim();

                foreach (var map in characterSounds)
                {
                    if (map.characterName.Trim().Equals(speakerName, System.StringComparison.OrdinalIgnoreCase) ||
                        speakerName.Contains(map.characterName.Trim()))
                    {
                        soundToPlay = map.soundKey;
                        break;
                    }
                }
            }
            else
            {
                foreach (var map in characterSounds)
                {
                    if (line.Text.StartsWith(map.characterName + ":"))
                    {
                        soundToPlay = map.soundKey;
                        break;
                    }
                }
            }
        }

        // We check if the Manager exists, and pull the clip directly from its library
        if (AudioManager.Instance != null)
        {
            AudioClip clip = AudioManager.Instance.GetClip(soundToPlay);

            if (clip != null)
            {
                lastSoundTime = Time.time;
                if (stopAudioSource)
                {
                    print("Stopping, then playing");
                    if (managedAudioSource.isPlaying)
                    {
                        managedAudioSource.Stop();
                    }

                    managedAudioSource.clip = clip;
                    managedAudioSource.volume = volume;
                    managedAudioSource.pitch = Random.Range(minPitch, maxPitch);
                    managedAudioSource.Play();
                }
                else
                {
                    print("Playing OneShot");
                    managedAudioSource.pitch = Random.Range(minPitch, maxPitch);
                    managedAudioSource.PlayOneShot(clip, volume);
                }
            }
        }

        return YarnTask.CompletedTask;
    }

    public override void OnLineDisplayComplete()
    {
        // Reset 
    }

    public override void OnLineWillDismiss()
    {
        if (managedAudioSource != null && managedAudioSource.isPlaying)
        {
            managedAudioSource.Stop();
        }
    }
}
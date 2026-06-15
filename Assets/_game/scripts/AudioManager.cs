using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public struct NamedAudioClip
    {
        public string clipName;
        public AudioClip clip;
    }

    [System.Serializable]
    public struct NamedAudioClipArray
    {
        public string groupName;
        public AudioClip[] clips;
    }

    [Header("Audio Players Components")]
    [SerializeField] private AudioSource EffectsSource;
    [SerializeField] private AudioSource AmbienceSource;

    [Header("Random Pitch Adjustment Range")]
    public float LowPitchRange = .95f;
    public float HighPitchRange = 1.05f;

    public static AudioManager Instance = null;

    [Header("Audio Library")]
    [SerializeField] private List<NamedAudioClip> audioLibrary = new List<NamedAudioClip>();

    [Header("Random Audio Library")]
    [SerializeField] private List<NamedAudioClipArray> randomAudioLibrary = new List<NamedAudioClipArray>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // <-- You must return here so the duplicate stops executing!
        }
    }

    private void Start()
    {
        // When the Main Menu scene loads, tell the persistent audio manager to play the menu track
        if (Instance != null)
        {
            Instance.PlayAmbience("LoopMusic");
        }
    }

    // Added an optional 'pitch' parameter defaulting to 1f
    public void PlaySound(string clipName, float volume = 1f, float pitch = 1f)
    {
        AudioClip clip = GetClip(clipName);
        if (clip != null)
        {
            EffectsSource.pitch = pitch;
            EffectsSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning($"Audio clip '{clipName}' not found in AudioManager.");
        }
    }

    // 2. Play a random sound from a group (Great for adding variation to repetitive sounds like typewriters)
    public void PlayRandomSound(string groupName, float volume = 1f)
    {
        AudioClip[] clips = GetRandomClips(groupName);

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"Random audio group '{groupName}' not found or empty.");
            return;
        }

        // Grab a random clip from the array
        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        // Slightly tweak pitch so it doesn't sound robotic
        float randomPitch = Random.Range(LowPitchRange, HighPitchRange);
        EffectsSource.pitch = randomPitch;

        EffectsSource.PlayOneShot(clip, volume);
    }

    // ==========================================
    // ===== AMBIENCE & HELPERS =================
    // ==========================================

    public void PlayAmbience(string clipName)
    {
        AudioClip newAmbience = GetClip(clipName);
        if (newAmbience != null)
        {
            AmbienceSource.clip = newAmbience;
            AmbienceSource.loop = true;
            AmbienceSource.Play();
        }
    }

    public void StopAmbience()
    {
        if (AmbienceSource.isPlaying) AmbienceSource.Stop();
    }

    public AudioClip GetClip(string targetName)
    {
        foreach (var item in audioLibrary)
        {
            if (item.clipName == targetName) return item.clip;
        }
        return null;
    }

    // Call this to smoothly switch between background tracks
    public void TransitionAmbience(string clipName, float fadeDuration = 1.5f)
    {
        AudioClip newAmbience = GetClip(clipName);
        if (newAmbience != null)
        {
            // Stop any existing fade routine to prevent "volume fighting"
            StopAllCoroutines();
            StartCoroutine(FadeAmbienceRoutine(newAmbience, fadeDuration));
        }
        else
        {
            Debug.LogWarning($"Ambience clip '{clipName}' not found.");
        }
    }

    private IEnumerator FadeAmbienceRoutine(AudioClip newAmbience, float duration)
    {
        float targetVolume = 0.3f; // Set this to your desired max volume
        float startVolume = AmbienceSource.volume;

        // 1. Fade out current
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            AmbienceSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }

        // 2. Switch clip
        AmbienceSource.Stop();
        AmbienceSource.clip = newAmbience;
        AmbienceSource.Play();

        // 3. Fade in new
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            AmbienceSource.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }

        AmbienceSource.volume = targetVolume;
    }

    // Internal helper to search the random library
    private AudioClip[] GetRandomClips(string targetName)
    {
        foreach (var item in randomAudioLibrary)
        {
            if (item.groupName == targetName) return item.clips;
        }
        return null;
    }
}
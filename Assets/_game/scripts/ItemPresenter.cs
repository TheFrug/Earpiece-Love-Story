using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPresenter : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup presenterGroup;       // entire panel, fades in/out
    public Image panelBackground;            // decorative frame behind item
    public Image itemImage;                  // the actual displayed item sprite

    [Header("Presentation Settings")]
    public float fadeDuration = 0.4f;
    public float holdDuration = 2.0f;

    [Header("Item Library")]
    public List<ItemEntry> items;

    private Dictionary<string, Sprite> itemDict;
    private Coroutine currentRoutine;

    void Awake()
    {
        // Dictionary lookup for item sprites
        itemDict = new Dictionary<string, Sprite>();
        foreach (var entry in items)
        {
            if (!itemDict.ContainsKey(entry.key))
            {
                itemDict.Add(entry.key, entry.sprite);
            }
        }

        // Make sure the panel starts invisible
        presenterGroup.alpha = 0f;
        presenterGroup.gameObject.SetActive(false);
    }

    public void ShowItem(string itemKey)
    {
        if (!itemDict.TryGetValue(itemKey, out Sprite sprite))
        {
            Debug.LogWarning($"ItemPresenter: no item found for key '{itemKey}'.");
            return;
        }

        itemImage.sprite = sprite;

        // Restart any running animation
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(PresentationRoutine());
    }

    private IEnumerator PresentationRoutine()
    {
        presenterGroup.gameObject.SetActive(true);

        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            presenterGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        presenterGroup.alpha = 1f;

        // Hold on screen
        yield return new WaitForSecondsRealtime(holdDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            presenterGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        presenterGroup.alpha = 0f;

        presenterGroup.gameObject.SetActive(false);
        currentRoutine = null;
    }
}

[System.Serializable]
public struct ItemEntry
{
    public string key;
    public Sprite sprite;
}

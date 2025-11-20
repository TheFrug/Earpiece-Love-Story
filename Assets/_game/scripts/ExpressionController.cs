using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpressionController : MonoBehaviour
{
    [Header("Date Expression System")]
    public Image dateImage;

    [Tooltip("Populate this with all your facial expressions. Keys = Yarn expression codes.")]
    public List<ExpressionEntry> expressions;

    private Dictionary<string, Sprite> expressionDictionary;

    void Awake() {
        expressionDictionary = new Dictionary<string, Sprite>();
        foreach (var entry in expressions) {
            if (!expressionDictionary.ContainsKey(entry.key)) {
                expressionDictionary.Add(entry.key, entry.sprite);
            }
        }
    }

    public void ChangeExpression(string key) {
        if (expressionDictionary.TryGetValue(key, out Sprite sprite)) {
            dateImage.sprite = sprite;
        } else {
            Debug.LogWarning($"ExpressionController: no expression found for key '{key}'.");
        }
    }
}

[System.Serializable]
public struct ExpressionEntry {
    public string key;
    public Sprite sprite;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Manages global game state and variables for YarnSpinner to access.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int confidence = 0;
    public int honesty = 0;
    public int bodyCount = 9;
    public enum frame{
		
		FirstSample,
		SecondSample,
		ThirdSample,
		
	}

    public frame frameStatus;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // --- Example Yarn-accessible methods ---

    [YarnCommand("add_confidence")]
    public void AddConfidence(int amount)
    {
        confidence += amount;
        Debug.Log($"Confidence is now {confidence}");
    }

    [YarnCommand("add_honesty")]
    public void AddHonesty(int amount)
    {
        honesty += amount;
        Debug.Log($"Honesty is now {honesty}");
    }

    [YarnFunction("get_confidence")]
    public static int GetConfidence()
    {
        return Instance.confidence;
    }

    [YarnFunction("get_honesty")]
    public static int GetHonesty()
    {
        return Instance.honesty;
    }
}
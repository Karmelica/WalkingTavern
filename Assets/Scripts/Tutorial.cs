using System.Collections.Generic;
using Cooking.ScriptableObjects;
using UnityEngine;

public static class Tutorial
{
	private static readonly Dictionary<string, string> TutorialDict = new();

	public static void CheckTutorial(string tutorialName)
	{
		PlayerPrefs.SetInt(tutorialName, 1);
	}

	private static void UncheckTutorial(string tutorialName)
	{
		PlayerPrefs.SetInt(tutorialName, 0);
	}

	public static string GetTutorialTextByName(string tutorialName)
	{
		TutorialDict.TryGetValue(tutorialName, out var value);
		return value;
	}

	public static void ResetTutorials()
	{
		foreach (var texts in TutorialDict) UncheckTutorial(texts.Key);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		var texts = Resources.Load<TutorialTexts>("ScriptableObjects/TutorialText/TutorialScript");
		foreach (var text in texts.texts) TutorialDict.TryAdd(text.tutorialName, text.tutorialText);
	}
}
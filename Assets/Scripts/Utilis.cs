using System.Text.RegularExpressions;
using UnityEngine;

public static class Utilis
{
	public static int CompareRaycastDistance(RaycastHit x, RaycastHit y)
	{
		return x.distance.CompareTo(y.distance);
	}

	private static string DeleteWord(string str, string delete)
	{
		return str.Replace(delete, "");
	}
	
	public static string ReplaceWordWith(this string str, string replace, string replaceWith)
	{
		return str.Replace(replace, replaceWith);
	}

	public static string SplitBigLetter(this string str)
	{
		//https://stackoverflow.com/questions/773303/splitting-camelcase
		return Regex.Replace(str, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
	}

	public static string DeleteAndSplit(this string str, string delete)
	{
		return SplitBigLetter(DeleteWord(str, delete));
	}

	public static void ShowSelectedMesh(this SkinnedMeshRenderer[] renderers, int selectedIndex)
	{
		for (var i = 0; i < renderers.Length; i++) renderers[i].enabled = i == selectedIndex;
	}
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utilis
{
    public static int CompareRaycastDistance(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }

    public static string DeleteWord(string str, string delete)
    {
        return str.Replace(delete, "");
    }

    public static string SplitBigLetter(string str)
    {
        //https://stackoverflow.com/questions/773303/splitting-camelcase
        return System.Text.RegularExpressions.Regex.Replace(str, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

    public static string DeleteAndSplit(string str, string delete)
    {
        return SplitBigLetter(DeleteWord(str, delete));
    }
    
    public static void ShowSelectedMesh(SkinnedMeshRenderer[] renderers, int selectedIndex)
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = i == selectedIndex;
        }
    }
}

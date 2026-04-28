using System.Collections.Generic;
using UnityEngine;

public static class Utilis
{
    private static readonly char[] BigLetters =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',  'N', 'O', 'P',  'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',  'Y', 'Z'
    };
    
    public static int CompareRaycastDistance(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }

    public static string DeleteAndSplit(string str, string delete)
    {
        //var replace = str.Replace(delete, "");
        //return replace.Split(BigLetters, 1);
        return str.Replace(delete, "");
    }
    
    public static void ShowSelectedMesh(SkinnedMeshRenderer[] renderers, int selectedIndex)
    {
        for (var i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = i == selectedIndex;
        }
    }
}

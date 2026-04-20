using System.Collections.Generic;
using UnityEngine;

public static class Utilis
{
    public static int CompareRaycastDistance(RaycastHit x, RaycastHit y)
    {
        return x.distance.CompareTo(y.distance);
    }
}

using UnityEngine;
using System.Collections;

public static class TypeUtility {
    public static T Cast<T>(T typeHodler, object x)
    {
        return (T)x;
    }
}

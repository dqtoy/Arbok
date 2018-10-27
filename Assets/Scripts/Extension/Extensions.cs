using System.Collections;
using UnityEngine;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods {
    public static bool HasComponent<T>(this GameObject obj) {
        return obj.GetComponent(typeof(T)) != null;
    }
    public static string Truncate(this string value, int maxLength) {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}

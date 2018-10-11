using UnityEngine;
using System.Collections;

//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    public static bool HasComponent<T>(this GameObject obj)
    {
        return obj.GetComponent(typeof(T)) != null;
    }
}
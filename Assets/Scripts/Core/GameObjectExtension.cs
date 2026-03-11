using UnityEngine;

public static class GameObjectExtension
{
    public static void SafeSetActive(this GameObject go, bool isActive)
    {
        if (go != null)
        {
            go.SetActive(isActive);
        }
    }
}
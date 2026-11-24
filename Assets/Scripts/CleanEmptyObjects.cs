using UnityEditor;
using UnityEngine;

public static class CleanEmptyObjects
{
    static bool IsJunk(GameObject go)
    {
        if (go.name.Length != 0) return false;
        if (go.transform.childCount != 0) return false;
        if (PrefabUtility.IsPartOfPrefabInstance(go)) return false;
        foreach (var c in go.GetComponents<Component>())
        {
            if (c == null) return false;
            if (!(c is Transform) && !(c is SpriteRenderer)) return false;
        }
        return true;
    }

    [MenuItem("Tools/Clean Empty Objects")]
    static void Clean()
    {
        var all = Object.FindObjectsByType<Transform>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var t in all) if (t != null && IsJunk(t.gameObject)) count++;

        if (!EditorUtility.DisplayDialog("Clean", $"Delete {count} junk objects?", "Delete", "Cancel"))
            return;

        foreach (var t in all)
            if (t != null && IsJunk(t.gameObject))
                Undo.DestroyObjectImmediate(t.gameObject);

        Debug.Log($"Deleted {count} junk objects.");
    }
}
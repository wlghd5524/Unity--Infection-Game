using UnityEditor;
using UnityEngine;

public class TagFilterWindow : EditorWindow
{
    private string tagToFilter = "Floor"; // 필터링할 태그 이름

    [MenuItem("Tools/Tag Filter")]
    public static void ShowWindow()
    {
        GetWindow<TagFilterWindow>("Tag Filter");
    }

    void OnGUI()
    {
        GUILayout.Label("Tag Filter", EditorStyles.boldLabel);
        tagToFilter = EditorGUILayout.TextField("Tag to Filter", tagToFilter);

        if (GUILayout.Button("Find and Select Objects with Tag"))
        {
            FindAndSelectObjectsWithTag();
        }
    }

    private void FindAndSelectObjectsWithTag()
    {
        if (string.IsNullOrEmpty(tagToFilter))
        {
            Debug.LogWarning("Please enter a valid tag.");
            return;
        }

        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tagToFilter);
        if (objectsWithTag.Length > 0)
        {
            Selection.objects = objectsWithTag;
            Debug.Log($"Found and selected {objectsWithTag.Length} objects with tag '{tagToFilter}'.");
        }
        else
        {
            Debug.LogWarning($"No objects with tag '{tagToFilter}' found in the scene.");
        }
    }
}

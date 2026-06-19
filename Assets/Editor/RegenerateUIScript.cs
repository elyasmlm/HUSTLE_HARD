using UnityEditor;
using UnityEditor.SceneManagement;

public static class RegenerateUIScript
{
    [MenuItem("Tools/CombatCoq/Regenerate UI Now")]
    public static void RegenerateUI()
    {
        // Charger la scène SampleScene
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

        // Exécuter la création de l'UI
        CreateCombatCoqUIEditor.CreateUI();

        // Sauvegarder la scène
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Succès", "L'UI du Combat de Coq a été régénérée!", "OK");
    }
}

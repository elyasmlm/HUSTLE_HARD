using UnityEngine;
using UnityEditor;

public class AssignTextures : EditorWindow
{
    [MenuItem("Tools/Assigner Textures Auto")]
    static void AssignerTextures()
    {
        string[] mats = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        int assignés = 0;
        int manquants = 0;

        foreach (string guid in mats)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            string matName = mat.name;

            string[] textures = AssetDatabase.FindAssets(matName + " t:Texture", new[] { "Assets/Textures" });

            if (textures.Length > 0)
            {
                string texPath = AssetDatabase.GUIDToAssetPath(textures[0]);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                mat.SetTexture("_BaseMap", tex);
                EditorUtility.SetDirty(mat);
                assignés++;
            }
            else
            {
                Debug.LogWarning("MANQUANT : " + matName);
                manquants++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Terminé ! Assignés : " + assignés + " | Manquants : " + manquants);
    }
}
using UnityEngine;
using UnityEditor;
using System.IO;

public static class SkinPrefabBuilderEditor
{
    private const string PREFAB_DIR = "Assets/MenuGame/Prefabs/Skins";

    private struct SkinDef
    {
        public string assetPath;
        public string prefabName;
    }

    private static readonly SkinDef[] SkinDefs = new SkinDef[]
    {
        new SkinDef { assetPath = "Assets/xxxtentacion 3d.fbx", prefabName = "XXXTentacionPreview" },
        new SkinDef { assetPath = "Assets/model.glb",           prefabName = "FemmePreview"        },
    };

    [MenuItem("Tools/MenuGame/Build Skin Prefabs")]
    public static void BuildSkinPrefabs()
    {
        if (!Directory.Exists(PREFAB_DIR))
        {
            Directory.CreateDirectory(PREFAB_DIR);
            AssetDatabase.Refresh();
        }

        int built = 0;

        foreach (SkinDef def in SkinDefs)
        {
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(def.assetPath);
            if (source == null)
            {
                Debug.LogWarning("[MenuGame] Asset introuvable : " + def.assetPath);
                continue;
            }

            string prefabPath = PREFAB_DIR + "/" + def.prefabName + ".prefab";

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = def.prefabName;

            bool success;
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath, out success);
            Object.DestroyImmediate(instance);

            if (success)
            {
                Debug.Log("[MenuGame] Prefab cree : " + prefabPath);
                built++;
            }
            else
            {
                Debug.LogError("[MenuGame] Echec creation prefab : " + prefabPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Build Skin Prefabs",
            built + " prefab(s) genere(s) dans\n" + PREFAB_DIR + "\n\n" +
            "Lancez ensuite :\nTools > MenuGame > Auto Populate Skins",
            "OK");
    }
}

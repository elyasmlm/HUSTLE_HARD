using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MenuGame;

public static class SkinSelectionAutoSetupEditor
{
    private const string MENU_ITEM  = "Tools/MenuGame/Auto Populate Skins";
    public  const string PREFAB_DIR = "Assets/MenuGame/Prefabs/Skins";

    private struct SkinConfig
    {
        public string  prefabName;
        public string  displayName;
        public Vector3 positionOffset;
        public Vector3 rotation;
        public Vector3 scale;
    }

    private static readonly SkinConfig[] Configs = new SkinConfig[]
    {
        new SkinConfig
        {
            prefabName     = "XXXTentacionPreview",
            displayName    = "Spencer Bouzelouf",
            positionOffset = Vector3.zero,
            rotation       = new Vector3(0f, 180f, 0f),
            scale          = Vector3.zero,
        },
        new SkinConfig
        {
            prefabName     = "FemmePreview",
            displayName    = "Marina ZigEtCharlot",
            positionOffset = Vector3.zero,
            rotation       = new Vector3(0f, 180f, 0f),
            scale          = Vector3.zero,
        },
    };

    // ── Appel depuis le menu Unity ─────────────────────────────────────────

    [MenuItem(MENU_ITEM)]
    public static void AutoPopulateSkins()
    {
        SkinSelectionController ctrl = FindSkinController();
        if (ctrl == null)
        {
            EditorUtility.DisplayDialog(
                "Auto Populate Skins",
                "Aucun SkinSelectionController trouve dans la scene.\n\n" +
                "Regenerez d'abord la scene via :\nTools > MenuGame > Create Main Menu Scene\n\n" +
                "La scene s'ouvre automatiquement apres la generation.",
                "OK");
            return;
        }

        EnsurePrefabsExist();
        PopulateController(ctrl, saveScene: true, showDialog: true);
    }

    // ── Appel programmatique depuis CreateMainMenuSceneEditor ─────────────

    public static void PopulateControllerDirect(SkinSelectionController ctrl)
    {
        EnsurePrefabsExist();
        PopulateController(ctrl, saveScene: false, showDialog: false);
    }

    // ── Logique commune ───────────────────────────────────────────────────

    private static void EnsurePrefabsExist()
    {
        bool missing = false;
        foreach (SkinConfig cfg in Configs)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB_DIR + "/" + cfg.prefabName + ".prefab") == null)
            {
                missing = true;
                break;
            }
        }

        if (missing)
            SkinPrefabBuilderEditor.BuildSkinPrefabs();
    }

    private static void PopulateController(SkinSelectionController ctrl, bool saveScene, bool showDialog)
    {
        List<SkinData> skins = new List<SkinData>();

        foreach (SkinConfig cfg in Configs)
        {
            string     path   = PREFAB_DIR + "/" + cfg.prefabName + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogWarning("[MenuGame] Prefab introuvable, skin ignore : " + path);
                continue;
            }

            skins.Add(new SkinData
            {
                displayName           = cfg.displayName,
                previewPrefab         = prefab,
                previewPositionOffset = cfg.positionOffset,
                previewRotation       = cfg.rotation,
                previewScale          = cfg.scale,
            });
        }

        if (skins.Count == 0)
        {
            Debug.LogWarning("[MenuGame] Aucun prefab de skin charge — verifiez Assets/xxxtentacion 3d.fbx et Assets/model.glb");
            return;
        }

        Undo.RecordObject(ctrl, "Auto Populate Skins");
        ctrl.skins = skins;
        EditorUtility.SetDirty(ctrl);

        if (saveScene && EditorSceneManager.GetActiveScene().isDirty)
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Auto Populate Skins",
                skins.Count + " skin(s) configures :\n\n" +
                "- XXXTentacion  (xxxtentacion 3d.fbx)\n" +
                "- Femme  (model.glb)\n\n" +
                "Appuyez sur Play pour tester.",
                "OK");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static SkinSelectionController FindSkinController()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<SkinSelectionController>();
#else
        return Object.FindObjectOfType<SkinSelectionController>();
#endif
    }
}



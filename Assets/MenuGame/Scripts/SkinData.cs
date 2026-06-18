using System;
using UnityEngine;

namespace MenuGame
{
    [Serializable]
    public class SkinData
    {
        [Tooltip("Nom affiche dans le menu. Si vide, le nom est derive de l'asset.")]
        public string displayName = "";

        [Tooltip("Prefab 3D instancie dans la zone de preview.")]
        public GameObject previewPrefab;

        [Header("Transform de preview")]
        [Tooltip("Decalage local applique au modele dans la zone de preview.")]
        public Vector3 previewPositionOffset = Vector3.zero;

        [Tooltip("Rotation locale appliquee au modele dans la zone de preview (euler).")]
        public Vector3 previewRotation = Vector3.zero;

        [Tooltip("Echelle locale appliquee au modele dans la zone de preview. (0,0,0) = auto-fit.")]
        public Vector3 previewScale = Vector3.zero;

        // ── Nom affiché ────────────────────────────────────────────────────────

        public string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(displayName))
                return displayName;

            string raw = previewPrefab != null ? previewPrefab.name : "";
            return string.IsNullOrEmpty(raw) ? "Skin" : CleanName(raw);
        }

        public static string CleanName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "Skin";

            string[] suffixes = {
                "_Prefab", "_prefab", "_Skin", "_skin",
                "_Mat", "_mat", "_Material", "_material",
                "_baseColor", "_BaseColor", "_albedo"
            };

            foreach (string suffix in suffixes)
            {
                if (raw.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    raw = raw.Substring(0, raw.Length - suffix.Length);
                    break;
                }
            }

            raw = raw.Replace("_", " ").Replace("-", " ");

            string[] words = raw.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }

            return string.Join(" ", words);
        }
    }
}


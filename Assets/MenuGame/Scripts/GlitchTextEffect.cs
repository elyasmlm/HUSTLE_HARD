using UnityEngine;
using TMPro;

namespace MenuGame
{

    [RequireComponent(typeof(TMP_Text))]
    public class GlitchTextEffect : MonoBehaviour
    {
        [Header("Couleur de base")]
        public Color normalColor = Color.white;

        private TMP_Text textMesh;

        void Awake()
        {
            textMesh = GetComponent<TMP_Text>();
            textMesh.color = normalColor;
            textMesh.alpha = 1f;
        }
    }
}

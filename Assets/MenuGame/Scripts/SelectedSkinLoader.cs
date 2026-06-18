using UnityEngine;

namespace MenuGame
{
    public class SelectedSkinLoader : MonoBehaviour
    {
        private const string PREF_SKIN = "SelectedSkinIndex";

        public int LoadedSkinIndex { get; private set; }

        void Awake()
        {
            LoadedSkinIndex = PlayerPrefs.GetInt(PREF_SKIN, 0);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MenuGame
{
    public class OptionsMenuController : MonoBehaviour
    {
        [Header("Références UI")]
        public Slider volumeSlider;
        public Toggle fullscreenToggle;
        public TMP_Dropdown qualityDropdown;

        private const string PREF_VOLUME = "MasterVolume";
        private const string PREF_FULLSCREEN = "Fullscreen";
        private const string PREF_QUALITY = "QualityLevel";

        void OnEnable()
        {
            LoadPreferences();
        }

        // ── Initialisation ─────────────────────────────────────────────────

        private void LoadPreferences()
        {
            float volume = PlayerPrefs.GetFloat(PREF_VOLUME, 1f);
            AudioListener.volume = volume;
            if (volumeSlider != null)
            {
                volumeSlider.value = volume;
                volumeSlider.onValueChanged.RemoveAllListeners();
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            bool fullscreen = PlayerPrefs.GetInt(PREF_FULLSCREEN, 1) == 1;
            Screen.fullScreen = fullscreen;
            if (fullscreenToggle != null)
            {
                fullscreenToggle.isOn = fullscreen;
                fullscreenToggle.onValueChanged.RemoveAllListeners();
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            int quality = PlayerPrefs.GetInt(PREF_QUALITY, QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(quality);
            if (qualityDropdown != null)
            {
                PopulateQualityDropdown();
                qualityDropdown.value = quality;
                qualityDropdown.onValueChanged.RemoveAllListeners();
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }
        }

        private void PopulateQualityDropdown()
        {
            if (qualityDropdown == null) return;
            qualityDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>(QualitySettings.names);
            qualityDropdown.AddOptions(options);
        }

        // ── Callbacks ──────────────────────────────────────────────────────

        public void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(PREF_VOLUME, value);
        }

        public void OnFullscreenChanged(bool value)
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt(PREF_FULLSCREEN, value ? 1 : 0);
        }

        public void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
            PlayerPrefs.SetInt(PREF_QUALITY, index);
        }

        public void OnBackClicked()
        {
            PlayerPrefs.Save();
            MainMenuController mainMenu = FindFirstObjectByType<MainMenuController>();
            if (mainMenu != null)
                mainMenu.ShowMainMenu();
        }
    }
}

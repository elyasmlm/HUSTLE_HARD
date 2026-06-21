using UnityEngine;
using UnityEngine.SceneManagement;

namespace MenuGame
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Scene de jeu")]
        public string gameplaySceneName = "SampleScene";

        [Header("Panneaux")]
        public GameObject optionsPanel;
        public GameObject skinPanel;
        public GameObject mainButtonsPanel;

        void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowMainMenu();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ShowMainMenu();
        }

        // -- Boutons principaux -------------------------------------------------

        public void OnPlayClicked()
        {
            IntroAppel.ReinitialiserIntro();
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void OnOptionsClicked()
        {
            SetPanelVisible(mainButtonsPanel, false);
            SetPanelVisible(skinPanel, false);
            SetPanelVisible(optionsPanel, true);
        }

        public void OnSkinClicked()
        {
            SetPanelVisible(mainButtonsPanel, false);
            SetPanelVisible(optionsPanel, false);
            SetPanelVisible(skinPanel, true);
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // -- Navigation ---------------------------------------------------------

        public void ShowMainMenu()
        {
            SetPanelVisible(mainButtonsPanel, true);
            SetPanelVisible(optionsPanel, false);
            SetPanelVisible(skinPanel, false);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // -- Utilitaire ---------------------------------------------------------

        private void SetPanelVisible(GameObject panel, bool visible)
        {
            if (panel != null)
                panel.SetActive(visible);
        }
    }
}
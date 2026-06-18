using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MenuGame
{
    public class SkinSelectionController : MonoBehaviour
    {
        private const string PREF_SKIN     = "SelectedSkinIndex";
        private const string PREF_NAME     = "SelectedSkinName";
        private const int    PREVIEW_LAYER = 31;

        [Header("Skins disponibles")]
        public List<SkinData> skins = new List<SkinData>();

        [Header("References UI")]
        public RawImage   previewRawImage;
        public TMP_Text   skinNameText;
        public TMP_Text   feedbackText;
        public GameObject noPreviewLabel;

        [Header("Preview 3D")]
        public Camera    previewCamera;
        public Transform previewAnchor;

        private int           currentIndex;
        private Coroutine     feedbackRoutine;
        private GameObject    currentPreviewInstance;
        private RenderTexture renderTex;

        void OnEnable()
        {
            EnsureRenderTexture();
            currentIndex = PlayerPrefs.GetInt(PREF_SKIN, 0);
            currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(0, skins.Count - 1));
            RefreshDisplay();
        }

        void OnDisable()
        {
            ClearPreview3D();
        }

        void OnDestroy()
        {
            if (renderTex != null)
            {
                if (previewCamera != null) previewCamera.targetTexture = null;
                renderTex.Release();
                Destroy(renderTex);
                renderTex = null;
            }
        }

        private void EnsureRenderTexture()
        {
            if (previewCamera == null || previewRawImage == null) return;
            if (renderTex != null) return;
            renderTex = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            renderTex.antiAliasing = 4;
            renderTex.Create();
            previewCamera.targetTexture = renderTex;
            previewRawImage.texture     = renderTex;
        }

        public void OnPreviousClicked()
        {
            if (skins.Count == 0) return;
            currentIndex = (currentIndex - 1 + skins.Count) % skins.Count;
            RefreshDisplay();
        }

        public void OnNextClicked()
        {
            if (skins.Count == 0) return;
            currentIndex = (currentIndex + 1) % skins.Count;
            RefreshDisplay();
        }

        public void OnSelectClicked()
        {
            PlayerPrefs.SetInt(PREF_SKIN, currentIndex);
            if (skins != null && currentIndex >= 0 && currentIndex < skins.Count)
                PlayerPrefs.SetString(PREF_NAME, skins[currentIndex].GetDisplayName());
            PlayerPrefs.Save();
            ShowFeedback("Skin selectionne !");
        }

        public void OnBackClicked()
        {
            MainMenuController mainMenu = FindFirstObjectByType<MainMenuController>();
            if (mainMenu != null)
                mainMenu.ShowMainMenu();
        }

        private void RefreshDisplay()
        {
            if (skins == null || skins.Count == 0)
            {
                SetNameText("Aucun skin");
                SetRawImageVisible(false);
                SetNoPreview(true);
                return;
            }
            SkinData current = skins[currentIndex];
            SetNameText(current.GetDisplayName());
            if (current.previewPrefab != null)
            {
                ShowPrefab3D(current);
                SetRawImageVisible(true);
                SetNoPreview(false);
            }
            else
            {
                ClearPreview3D();
                SetRawImageVisible(false);
                SetNoPreview(true);
            }
        }

        private void SetNameText(string v)
        {
            if (skinNameText != null) skinNameText.text = v;
        }

        private void SetRawImageVisible(bool visible)
        {
            if (previewRawImage != null) previewRawImage.gameObject.SetActive(visible);
        }

        private void SetNoPreview(bool visible)
        {
            if (noPreviewLabel != null) noPreviewLabel.SetActive(visible);
        }

        private void ShowPrefab3D(SkinData data)
        {
            ClearPreview3D();
            if (previewAnchor == null || data.previewPrefab == null) return;
            currentPreviewInstance = Instantiate(data.previewPrefab, previewAnchor.position, previewAnchor.rotation, previewAnchor);
            SetLayerRecursive(currentPreviewInstance, PREVIEW_LAYER);
            bool hasCustomScale    = data.previewScale    != Vector3.zero;
            bool hasCustomRotation = data.previewRotation != Vector3.zero;
            currentPreviewInstance.transform.localPosition = data.previewPositionOffset;
            currentPreviewInstance.transform.localRotation = hasCustomRotation ? Quaternion.Euler(data.previewRotation) : Quaternion.identity;
            currentPreviewInstance.transform.localScale    = hasCustomScale    ? data.previewScale                      : Vector3.one;
            if (!hasCustomScale) FitIntoPreviewBounds(currentPreviewInstance);
            if (currentPreviewInstance.GetComponent<SkinPreviewRotator>() == null)
                currentPreviewInstance.AddComponent<SkinPreviewRotator>();
        }

        private void ClearPreview3D()
        {
            if (currentPreviewInstance != null)
            {
                Destroy(currentPreviewInstance);
                currentPreviewInstance = null;
            }
        }

        private static void SetLayerRecursive(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layer;
        }

        private static void FitIntoPreviewBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxSize > 0f) go.transform.localScale = Vector3.one * (1.8f / maxSize);
        }

        private void ShowFeedback(string message)
        {
            if (feedbackText == null) return;
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(FeedbackRoutine(message));
        }

        private IEnumerator FeedbackRoutine(string message)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            feedbackText.gameObject.SetActive(false);
        }
    }
}

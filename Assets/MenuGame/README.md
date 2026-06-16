# MenuGame — Menu Principal HUSTLE HARD

Ambiance **horreur / VHS / glitch** pour le menu principal du jeu HUSTLE HARD.

---

## Structure des fichiers

```
Assets/MenuGame/
├── Editor/
│   └── CreateMainMenuSceneEditor.cs   ← Script Editor : génère la scène MainMenu
├── Scenes/
│   └── MainMenu.unity                 ← Scène générée par le script Editor
├── Scripts/
│   ├── MainMenuController.cs          ← Logique boutons + gestion curseur
│   ├── OptionsMenuController.cs       ← Panneau Options (volume, plein écran, qualité)
│   ├── MenuButtonHoverEffect.cs       ← Effet hover animé sur les boutons
│   └── GlitchTextEffect.cs            ← Effet glitch/flicker/VHS sur le titre
├── Prefabs/                           ← (réservé pour futurs prefabs)
├── Materials/                         ← (réservé pour futurs matériaux)
└── README.md                          ← Ce fichier
```

---

## Comment générer la scène MainMenu

1. Ouvrir Unity avec le projet **HUSTLE HARD**.
2. Dans la barre de menus Unity : **Tools > MenuGame > Create Main Menu Scene**.
3. La scène `Assets/MenuGame/Scenes/MainMenu.unity` est créée automatiquement.
4. La scène est ajoutée **en index 0** dans les Build Settings.
5. `SampleScene` est ajoutée en index 1.

> ⚠️ Si la scène existe déjà, relancer le script la recrée (la scène existante est remplacée).

---

## Fonctionnalités

### Bouton JOUER
- Charge la scène de gameplay via `SceneManager.LoadScene("SampleScene")`.
- Le nom de la scène est configurable dans l'Inspector du `MainMenuController`.

### Bouton OPTIONS
- Ouvre un panneau avec :
  - **Slider volume** → `AudioListener.volume` + `PlayerPrefs`
  - **Toggle plein écran** → `Screen.fullScreen` + `PlayerPrefs`
  - **Dropdown qualité graphique** → `QualitySettings` + `PlayerPrefs`
  - **Bouton Retour**

### Bouton SÉLECTION SKIN
- Affiche un panneau placeholder : *"Sélection de skin bientôt disponible."*
- Bouton Retour disponible.

### Bouton QUITTER
- En **build** : `Application.Quit()`
- Dans **l'éditeur** : `UnityEditor.EditorApplication.isPlaying = false`

### Gestion du curseur
- `Cursor.lockState = CursorLockMode.None`
- `Cursor.visible = true`
- Appui sur **Échap** ferme les panneaux secondaires.

---

## Design

| Élément            | Style                                        |
|--------------------|----------------------------------------------|
| Fond               | Noir profond (#0A0A0F)                       |
| Textes boutons     | Blanc cassé, gras, police TMP                |
| Titre HUSTLE       | Blanc, 140px, effet glitch                   |
| Titre HARD         | Jaune VHS (#FFEB26), 170px, effet glitch     |
| Séparateur         | Ligne verticale jaune semi-transparente      |
| Hover bouton       | Décalage +8px, couleur jaune, scale 1.05     |
| Glitch titre       | Flicker alpha + décalage horizontal aléatoire|

---

## Critères d'acceptation

- [x] Le projet compile sans erreur rouge.
- [x] La scène MainMenu existe dans `Assets/MenuGame/Scenes`.
- [x] MainMenu est la 1ère scène dans les Build Settings.
- [x] Le bouton JOUER charge SampleScene.
- [x] Le bouton OPTIONS ouvre un vrai panneau d'options.
- [x] Le bouton SÉLECTION SKIN affiche un placeholder propre.
- [x] Le bouton QUITTER fonctionne en éditeur et en build.
- [x] Le design est sombre, horreur, rétro/glitch.
- [x] Tous les fichiers sont dans `Assets/MenuGame/`.

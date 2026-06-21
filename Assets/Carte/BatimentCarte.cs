using UnityEngine;

/// <summary>
/// A placer sur chaque batiment a afficher sur la carte.
/// Le CarteManager les detecte automatiquement au demarrage.
/// </summary>
public class BatimentCarte : MonoBehaviour
{
    [Tooltip("Nom affiche sur la carte.")]
    public string nomBatiment = "Batiment";

    [Tooltip("Couleur du marqueur sur la carte.")]
    public Color couleur = new Color(0.85f, 0.75f, 0.30f, 1f);

    [Tooltip("Optionnel : repere place au centre visuel du batiment. " +
             "Utile quand le pivot du modele est decale. Si vide, on utilise la position de cet objet.")]
    public Transform pointReference;

    /// <summary>Transform a utiliser pour positionner le marqueur sur la carte.</summary>
    public Transform TransformCarte => pointReference != null ? pointReference : transform;
}

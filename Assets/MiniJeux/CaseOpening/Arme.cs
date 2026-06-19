/// <summary>
/// Rarete d'une arme dans le systeme de case opening.
/// </summary>
public enum RareteArme
{
    Normale,
    Rare,
    Mythique,
    Legendaire,
    Antique,
    ExtraRare
}

/// <summary>
/// Type de caisse disponible a l'achat.
/// </summary>
public enum TypeCaisse
{
    Normale,
    Supersonique
}

/// <summary>
/// Donnees d'une arme obtenue a l'ouverture d'une caisse.
/// </summary>
[System.Serializable]
public class Arme
{
    public string nom;
    public RareteArme rarete;
    public float valeur;
    public TypeCaisse caisse;

    public Arme(string nom, RareteArme rarete, float valeur, TypeCaisse caisse)
    {
        this.nom = nom;
        this.rarete = rarete;
        this.valeur = valeur;
        this.caisse = caisse;
    }
}
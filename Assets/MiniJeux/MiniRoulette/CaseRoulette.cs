/// <summary>
/// Types de lots possibles sur la mini-roulette.
/// </summary>
public enum TypeLot
{
    Perdu,
    NouvellePartieGratuite,
    Gain100,
    Gain200,
    Gain500,
    Gain1000,
    MultiplicateurX2,
    MultiplicateurX3
}

/// <summary>
/// Definition d'une case de la roulette avec son type, sa description et sa probabilite.
/// </summary>
[System.Serializable]
public class CaseRoulette
{
    public TypeLot type;
    public string nom;
    public string description;
    public float probabilite;

    public CaseRoulette(TypeLot type, string nom, string description, float probabilite)
    {
        this.type = type;
        this.nom = nom;
        this.description = description;
        this.probabilite = probabilite;
    }
}
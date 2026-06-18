/// <summary>
/// Types de lots possibles sur la mini-roulette.
/// </summary>
public enum TypeLot
{
    RienDuTout,
    NouvellePartieGratuite,
    PetitGainArgent,
    MultiplicateurX2,
    MultiplicateurX3,
    ChapeauCommun,
    ChapeauRare,
    TechniqueTriche
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
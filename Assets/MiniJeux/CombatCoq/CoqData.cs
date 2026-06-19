/// <summary>
/// Donnees d'un coq pour le mini-jeu Combat de Coq.
/// </summary>
[System.Serializable]
public class CoqData
{
    public string nom;
    public float cote;
    public bool energise;

    public CoqData(string nom, float cote)
    {
        this.nom = nom;
        this.cote = cote;
        this.energise = false;
    }
}
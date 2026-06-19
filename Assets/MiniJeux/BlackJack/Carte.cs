/// <summary>
/// Represente une carte a jouer.
/// </summary>
[System.Serializable]
public class Carte
{
    public string nom;
    public string symbole;
    public int valeur;

    public Carte(string nom, string symbole, int valeur)
    {
        this.nom = nom;
        this.symbole = symbole;
        this.valeur = valeur;
    }

    public override string ToString()
    {
        return nom + symbole;
    }
}
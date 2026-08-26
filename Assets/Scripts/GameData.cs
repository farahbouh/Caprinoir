using UnityEngine;

public static class GameData
{
    public static int niveauSauvegarde = 1;
    public static int xpSauvegarde = 0;
    public static int palierSauvegarde = 45;
    public static int pvActuelsSauvegarde = -1; // -1 signifie "pas encore de PV sauvés"
    public static bool estInitialise = false; // Très important !
    // AJOUTE CETTE LIGNE ICI :
    public static string prochainSpawnID = "";
}
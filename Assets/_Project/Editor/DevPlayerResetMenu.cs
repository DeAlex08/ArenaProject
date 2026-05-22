using UnityEditor;

public static class DevPlayerResetMenu
{
    [MenuItem("ArenaProject/Dev/Reset Player Save To Fresh New Player")]
    public static void ResetPlayerSaveToFreshNewPlayer()
    {
        PlayerSaveManager.ResetToFreshNewPlayerSave();
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkipTool : MonoBehaviour
{
    public string normalLevelScene = "Level_1";
    public string pawnScene = "Pawn_Boss";
    public string knightScene = "Knigh_Boss";
    public string bishopScene = "Bishop_Boss";
    public string rookScene = "Rook_Boss";

    private bool godModeActive = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
            if (manager != null)
            {
                manager.AdvanceLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) CheckerboardTransition.LoadScene(normalLevelScene);
        if (Input.GetKeyDown(KeyCode.Alpha1)) CheckerboardTransition.LoadScene(pawnScene);
        if (Input.GetKeyDown(KeyCode.Alpha2)) CheckerboardTransition.LoadScene(knightScene);
        if (Input.GetKeyDown(KeyCode.Alpha3)) CheckerboardTransition.LoadScene(bishopScene);
        if (Input.GetKeyDown(KeyCode.Alpha4)) CheckerboardTransition.LoadScene(rookScene);

        if (Input.GetKeyDown(KeyCode.O))
        {
            ApplyGodModeStats();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            godModeActive = !godModeActive;
            Debug.Log("Modo OP: " + godModeActive);
        }

        if (godModeActive)
        {
            ApplyGodModeStats();
        }
    }

    private void ApplyGodModeStats()
    {
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(9999f);
        }

        MagicShooter playerWeapon = Object.FindFirstObjectByType<MagicShooter>();
        if (playerWeapon != null)
        {
            playerWeapon.RefillManaToMax();
        }
    }
}
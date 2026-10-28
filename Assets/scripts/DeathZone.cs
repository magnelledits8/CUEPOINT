using UnityEngine;
using UnityEngine.SceneManagement; // Krävs för att ladda om banan

public class DeathZone : MonoBehaviour
{
    // Denna funktion körs direkt när något rör vid vår osynliga box
    private void OnTriggerEnter(Collider other)
    {
        // Vi kollar om det som trillade ner faktiskt är spelaren
        // (Se till att din spelare har tagen "Player" längst upp i Inspectorn!)
        if (other.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    private void RestartLevel()
    {
        // Hämtar namnet på den bana du är på just nu
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Laddar om hela banan från sekund noll (startar om sekvensen och musiken)
        SceneManager.LoadScene(currentSceneName);
    }
}

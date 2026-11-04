
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Klickljud")]
    public AudioClip clickSound;

    // Lärarens metod för att ladda valfri scen
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // PLAY - Startar spelet
    public void PlayGame()
    {
        PlayClickSound();
        LoadScene("Level1");
    }

    // MENU - Öppnar huvudmenyn
    public void OpenMenu()
    {
        PlayClickSound();
        LoadScene("Menu");
    }

    // QUIT - Avslutar spelet
    public void QuitGame()
    {
        PlayClickSound();
        Debug.Log("Spelet avslutas...");
        Application.Quit();
    }

    // Spelar klickljudet
    private void PlayClickSound()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clickSound,
                Camera.main != null ? Camera.main.transform.position : Vector3.zero
            );
        }
    }
}

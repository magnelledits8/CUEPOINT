
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu;

    [Header("Settings (Optional)")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Player Controls")]
    [SerializeField] private MouseLook mouseLook;
    [SerializeField] private PlayerShooting playerShooting;

    [Header("Scene Names")]
    [SerializeField] private string gameplayScene = "Level1";
    [SerializeField] private string menuScene = "Menu";

    [Header("Controls")]
    [SerializeField] private bool useEscapeKey = true;

    private bool isPaused;
    private bool isResuming;
    private bool settingsOpen;

    private bool mouseLookWasEnabled;
    private bool shootingWasEnabled;

    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;

    private Coroutine resumeRoutine;

    private void Awake()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!useEscapeKey || isResuming)
            return;

        if (Keyboard.current == null ||
            !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (settingsOpen)
        {
            BackFromSettings();
        }
        else if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (isPaused || isResuming || pauseMenu == null)
            return;

        isPaused = true;

        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;

        // Save which player controls are active
        if (mouseLook != null)
        {
            mouseLookWasEnabled = mouseLook.enabled;
            mouseLook.enabled = false;
        }

        if (playerShooting != null)
        {
            shootingWasEnabled = playerShooting.enabled;
            playerShooting.enabled = false;
        }

        // Pause the game and sound
        Time.timeScale = 0f;
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pauseMenu.SetActive(true);
    }

    public void ContinueGame()
    {
        if (!isPaused || isResuming)
            return;

        isResuming = true;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        settingsOpen = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        // Resume the game and sound
        // Wait before enabling player controls
        Time.timeScale = 1f;
        AudioListener.pause = false;

        resumeRoutine = StartCoroutine(
            ResumeControlsSafely()
        );
    }

    private IEnumerator ResumeControlsSafely()
    {
        // Wait until the mouse button is released
        // This also works while the game is paused
        while (Mouse.current != null &&
               Mouse.current.leftButton.isPressed)
        {
            yield return null;
        }

        // Wait one more frame
        // Stop the menu click from shooting
        yield return null;

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;

        // Skip the first frame after locking the mouse
        yield return null;

        RestorePlayerControls();

        isPaused = false;
        isResuming = false;
        resumeRoutine = null;
    }

    private void RestorePlayerControls()
    {
        if (mouseLook != null)
            mouseLook.enabled = mouseLookWasEnabled;

        if (playerShooting != null)
            playerShooting.enabled = shootingWasEnabled;
    }

    public void RestartGame()
    {
        ResetPauseState();
        SceneManager.LoadScene(gameplayScene);
    }

    public void BackToMenu()
    {
        ResetPauseState();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(menuScene);
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        if (!isPaused)
            PauseGame();

        settingsOpen = true;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void BackFromSettings()
    {
        settingsOpen = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pauseMenu != null && isPaused)
            pauseMenu.SetActive(true);
    }

    private void ResetPauseState()
    {
        if (resumeRoutine != null)
        {
            StopCoroutine(resumeRoutine);
            resumeRoutine = null;
        }

        RestorePlayerControls();

        isPaused = false;
        isResuming = false;
        settingsOpen = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnDisable()
    {
        if (isPaused || isResuming)
            ResetPauseState();
    }
}

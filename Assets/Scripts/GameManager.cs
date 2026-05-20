using UnityEngine;
using UnityEngine.SceneManagement;
using RPGInterfaces;
using RPG.Core;

/// <summary>
/// Manages game-wide functionality including pause/resume, scene loading, and application quit.
/// Uses the new InputSystem to detect pause input and controls Time.timeScale for pause mechanics.
/// Implements IGameManager interface for dependency injection.
/// </summary>
public class GameManager : MonoBehaviour, IGameManager
{
    private PlayerControls playerControls;
    [SerializeField] private GameObject pauseMenu;
    // Variable para saber si el juego está pausado
    private bool isPaused = false;

    /// <summary>
    /// Initializes the player controls input system on awake and registers service.
    /// </summary>
    private void Awake()
    {
        playerControls = new PlayerControls();
        ServiceLocator.RegisterService<IGameManager>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.UnregisterService<IGameManager>();
    }

    /// <summary>
    /// Enables the player input controls and subscribes to game events when the script is enabled.
    /// </summary>
    private void OnEnable()
    {
        playerControls.Enable();
        GameEvents.OnBattleTriggered += HandleBattleTriggered;
        GameEvents.OnBattleResolved += HandleBattleResolved;
        GameEvents.OnGameWon += HandleGameWon;
    }

    /// <summary>
    /// Disables the player input controls and unsubscribes from game events when the script is disabled.
    /// </summary>
    private void OnDisable()
    {
        playerControls.Disable();
        GameEvents.OnBattleTriggered -= HandleBattleTriggered;
        GameEvents.OnBattleResolved -= HandleBattleResolved;
        GameEvents.OnGameWon -= HandleGameWon;
    }

    private void HandleBattleTriggered()
    {
        LoadScene("BattleScene");
    }

    private void HandleBattleResolved(BattleResult result)
    {
        if (result == BattleResult.Won || result == BattleResult.Run)
        {
            LoadScene("RedDungeonLVL");
        }
        else if (result == BattleResult.Lost)
        {
            // The BattleSystem UI currently handles the GameOver display.
            // In the future, we could load a GameOver scene here.
        }
    }

    private void HandleGameWon()
    {
        LoadScene("WinScene");
    }

    /// <summary>
    /// Sets up the pause input listener if not in the main menu scene.
    /// </summary>
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == GameConstants.SCENE_MAIN_MENU)
        {
            return;
        }

        playerControls.Player.Pause.performed += _ => PauseGame();
    }

    /// <summary>
    /// Loads a new scene by name and resumes normal game time if paused.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        if (isPaused) isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Pauses the game by setting Time.timeScale to 0 and displays the pause menu.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenu) pauseMenu.SetActive(true);
    }

    /// <summary>
    /// Resumes the game by setting Time.timeScale to 1 and hides the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenu) pauseMenu.SetActive(false);
    }

    /// <summary>
    /// Closes the application or stops play mode in the editor.
    /// In the Unity editor, this stops play mode. In a build, it quits the application.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Cierra la aplicación en el build
            Application.Quit();
#endif
    }

    /// <summary>
    /// Toggles the pause state between paused and resumed.
    /// Calls either ResumeGame or PauseGame depending on the current state.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
}

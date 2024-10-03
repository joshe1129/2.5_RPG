using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerControls playerControls;
    [SerializeField] private GameObject pauseMenu;
    // Variable para saber si el juego está pausado
    private bool isPaused = false;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            return;
        }

        playerControls.Player.PauseGame.performed += _ => PauseGame();
    }

    // Método para cargar una nueva escena
    public void LoadScene(string sceneName)
    {
        // Cambia el estado de pausa
        if (isPaused) isPaused = false;
        // Reanuda el tiempo en el juego
        Time.timeScale = 1f;
        // Carga la escena especificada
        SceneManager.LoadScene(sceneName);
    }

    // Método para pausar el juego
    public void PauseGame()
    {
        // Cambia el estado de pausa
        isPaused = true;
        // Detiene el tiempo en el juego
        Time.timeScale = 0f;
        if (pauseMenu) pauseMenu.SetActive(true);
    }

    // Método para despausar el juego
    public void ResumeGame()
    {
        // Cambia el estado de pausa
        isPaused = false;
        // Reanuda el tiempo en el juego
        Time.timeScale = 1f;
    }

    // Método para cerrar el juego
    public void QuitGame()
    {
        // Si estamos en el editor, solo detiene la ejecución
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Cierra la aplicación en el build
            Application.Quit();
#endif
    }

    // Método para alternar entre pausar y despausar el juego
    public void TogglePause()
    {
        // Si el juego está pausado, lo despausamos
        if (isPaused)
        {
            ResumeGame();
        }
        // Si el juego no está pausado, lo pausamos
        else
        {
            PauseGame();
        }
    }
}

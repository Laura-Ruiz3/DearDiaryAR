using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShuffleUIManager : MonoBehaviour
{
    public GameObject livesBox;
    public GameObject instructionsBox;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public Button restartButton;
    public Button continueButton;

    private ShuffleGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<ShuffleGameManager>();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        continueButton.onClick.AddListener(OnContinueButtonPressed);

        HideWinPanel();
        HideGameOverPanel();
    }

    void OnRestartButtonPressed()
    {
        /*if (gameManager != null)
            gameManager.RestartGame();*/
        RestartGame();
    }

    /// <summary>
    /// Actualiza la UI con la cantidad de vidas actuales.
    /// </summary>
    //public void UpdateLives(int lives)
    //{
    //    if (livesText != null)
    //        livesText.text = "Vidas: " + lives;
    //}

    /// <summary>
    /// Muestra el panel de Game Over.
    /// </summary>
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        //if (livesText != null)
        //    livesText.gameObject.SetActive(false);
        if (instructionsBox != null)
            instructionsBox.SetActive(false);
        Debug.Log("UIManagerAR: Mostrando pantalla de Game Over.");
    }

    /// <summary>
    /// Oculta el panel de Game Over.
    /// </summary>
    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        Debug.Log("UIManagerAR: Ocultando pantalla de Game Over.");
    }

    /// <summary>
    /// Muestra el panel de Victoria.
    /// </summary>
    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        if (instructionsBox != null)
            instructionsBox.SetActive(false);
        Debug.Log("UIManagerAR: Mostrando pantalla de Victoria.");
    }

    /// <summary>
    /// Oculta el panel de Victoria.
    /// </summary>
    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
        Debug.Log("UIManagerAR: Ocultando pantalla de Victoria.");
    }

    /// <summary>
    /// Recarga la escena actual.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("UIManagerAR: Recargando escena.");
        // Puedes poner el nombre exacto de tu escena, o usar el índice actual:
        //SceneManager.LoadScene("V4");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnContinueButtonPressed()
    {
        // Puedes cambiar "NombreDeLaSiguienteEscena" por la escena que desees cargar.
        Debug.Log("UIManagerAR: Botón Continuar presionado.");
        SceneManager.LoadScene("V3");
    }
}

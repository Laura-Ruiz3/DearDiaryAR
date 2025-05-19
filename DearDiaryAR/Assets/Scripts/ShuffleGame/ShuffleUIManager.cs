using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//Muestra la interfaz gráfica del juego
public class ShuffleUIManager : MonoBehaviour
{
    public GameObject livesBox; //Contiene los elementos gráficos para mostrar las vidas
    public GameObject instructionsBox; //Contiene los elementos gráficos para mostrar las instrucciones
    public GameObject gameOverPanel; //Pantalla de derrota
    public GameObject winPanel; //Pantalla de victoria
    public Button restartButton; //Botón de reinicio
    public Button continueButton; //Botón para continuar

    private ShuffleGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<ShuffleGameManager>();
        restartButton.onClick.AddListener(OnRestartButtonPressed);
        continueButton.onClick.AddListener(OnContinueButtonPressed);

        HideWinPanel();
        HideGameOverPanel();
    }

    //Inicia el comportamiento de restart
    void OnRestartButtonPressed()
    {
        RestartGame();
    }

    //Muestra la pantalla de derrota
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (instructionsBox != null)
            instructionsBox.SetActive(false);
    }

    //Oculta la pantalla de derrota
    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    //Muestra la pantalla de victoria
    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        if (instructionsBox != null)
            instructionsBox.SetActive(false);
    }

    //Oculta la pantalla de victoria
    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }
    
    //Reinicia el juego
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //Avanza a la siguiente escena (menú principal)
    void OnContinueButtonPressed()
    {
        SceneManager.LoadScene("V3");
    }
}

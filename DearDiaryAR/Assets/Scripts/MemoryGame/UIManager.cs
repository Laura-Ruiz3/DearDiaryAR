using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

//Muestra la interfaz gráfica del juego
public class UIManager : MonoBehaviour
{
    public GameObject livesBox; //Contiene los elementos gráficos para mostrar las vidas
    public GameObject instructionsBox; //Contiene los elementos gráficos para mostrar las instrucciones
    public GameObject gameOverPanel; //Pantalla de derrota
    public GameObject winPanel; //Pantalla de victoria
    public UnityEngine.UI.Button restartButton; //Botón de reinicio
    public UnityEngine.UI.Button continueButton; //Botón para continuar

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        restartButton.onClick.AddListener(OnRestartButtonPressed);        
        continueButton.onClick.AddListener(OnContinueButtonPressed);

        HideWinPanel();
        HideGameOverPanel();
    }

    //Inicia el comportamiento de restart
    void OnRestartButtonPressed()
    {
        if (gameManager != null)
            RestartGame();
    }

    //Actualiza en la textbox el número de vidas
    public void UpdateLives(int lives)
    {
        TMP_Text livesText = livesBox.GetComponentInChildren<TMP_Text>();
        if (livesText != null)
            livesText.text = "Vidas: " + lives;
    }

    //Muestra la pantalla de derrota
    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (livesBox != null)
            livesBox.gameObject.SetActive(false);
        if (instructionsBox != null)
            instructionsBox.gameObject.SetActive(false);
    }

    //Oculta la pantalla de derrota
    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    //Reinicia el juego
    public void RestartGame()
    {
        SceneManager.LoadScene("V5");
    }

    //Muestra la pantalla de victoria
    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        if (livesBox != null)
            livesBox.gameObject.SetActive(false);
        if (instructionsBox != null)
            instructionsBox.gameObject.SetActive(false);
    }

    //Oculta la pantalla de victoria
    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    //Avanza a la siguiente escena
    void OnContinueButtonPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("V6.5");
    }
}

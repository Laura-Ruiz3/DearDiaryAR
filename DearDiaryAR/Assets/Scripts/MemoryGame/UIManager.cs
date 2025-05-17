using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    //public TMP_Text livesText;
    public GameObject livesBox;
    public GameObject instructionsBox;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button continueButton;    
    //public GameObject loseImagePanel;
    //public float loseImageDuration = 2f; // tiempo que estará visible la imagen
    //public AudioSource loseAudioSource;
    //public AudioClip loseClip;


    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        /*restartButton.onClick.AddListener(() =>
        {
            if (gameManager != null)
                gameManager.RestartGame();
        });*/
        restartButton.onClick.AddListener(OnRestartButtonPressed);        
        continueButton.onClick.AddListener(OnContinueButtonPressed);

        HideWinPanel();
        HideGameOverPanel();
    }

    void OnRestartButtonPressed()
    {
        if (gameManager != null)
            RestartGame();
    }

    public void UpdateLives(int lives)
    {
        TMP_Text livesText = livesBox.GetComponentInChildren<TMP_Text>();
        if (livesText != null)
            livesText.text = "Vidas: " + lives;
    }

    public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        if (livesBox != null)
            livesBox.gameObject.SetActive(false);
        if (instructionsBox != null)
            instructionsBox.gameObject.SetActive(false);
    }

    public void HideGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void RestartGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Recargando escena");
        SceneManager.LoadScene("V5");

    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(true);
        if (livesBox != null)
            livesBox.gameObject.SetActive(false);
        if (instructionsBox != null)
            instructionsBox.gameObject.SetActive(false);
    }

    public void HideWinPanel()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void OnContinueButtonPressed()
    {
        // Carga la siguiente escena, por ejemplo:
        UnityEngine.SceneManagement.SceneManager.LoadScene("V6.5");
    }
}

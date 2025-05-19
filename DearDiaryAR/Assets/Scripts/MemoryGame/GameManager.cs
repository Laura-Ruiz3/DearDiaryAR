using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Vuforia;

//Se encarga de toda la lógica del juego (ejecución y condiciones de victoria y derrota)
public class GameManager : MonoBehaviour
{
    //Cantidad de vidas
    public int maxLives = 5;
    private int currentLives;

    //Llamada a interfaz gráfica
    public UIManager uiManager;

    //Cartas seleccionadas
    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    private bool canSelect = true;

    //Marcador a detectar
    public GameObject marcador;

    //Audio del juego
    public AudioSource musicSource;
    public AudioClip fullLivesClip;
    public AudioClip mediumLivesClip;
    public AudioClip lowLivesClip;
    public AudioClip loseClip;
    public AudioClip winClip;

    //Modelo animado mostrado en condicipon de derrota
    public GameObject loseModel;
    public Animator loseAnimator;
    public string loseAnimationName = "Lose";
    public AudioSource loseAudioSource;
    public float loseAnimationDuration = 10f;

    public GameObject specialModel;

    private AudioClip currentClip;

    void Start()
    {
        currentLives = maxLives;
        uiManager.UpdateLives(currentLives);
        uiManager.HideGameOverPanel();
        if (marcador != null && !marcador.activeSelf)
            marcador.SetActive(true);
        if (musicSource != null && fullLivesClip != null)
        {
            musicSource.clip = fullLivesClip;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (!canSelect) return; 

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            DetectTouch(Input.GetTouch(0).position);
        }
    }


    //Función que ayuda a que se detecte el toque en pantalla a las cartas
    void DetectTouch(Vector2 screenPos)
    {    
        Camera arCamera = Camera.main;
        if (arCamera == null)
        {
            Debug.LogWarning("No se encontró la cámara principal para el Raycast.");
            return;
        }
    
        Ray ray = arCamera.ScreenPointToRay(screenPos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {    
            Card card = hit.collider.GetComponentInParent<Card>();
            if (card != null)
            {
                card.OnSelected();
            }
        }
        else
        {
            Debug.Log("Raycast no tocó nada.");
        }
    }
    
    /*Acciones a realizar cuando se selecciona un par de cartas
     * Args:
     *  card: carta que se ha seleccionado         
     */
    public void OnCardSelected(Card card)
    {
        if (!canSelect || card == firstSelectedCard || card == secondSelectedCard)
            return;

        if (firstSelectedCard == null)
        {
            firstSelectedCard = card;
        }
        else if (secondSelectedCard == null)
        {
            secondSelectedCard = card;
            canSelect = false;
            StartCoroutine(CheckMatch());
        }
    }

    //Comprueba si se ha formado un par
    private IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstSelectedCard.cardId == secondSelectedCard.cardId)
        {
            firstSelectedCard.SetMatched();
            secondSelectedCard.SetMatched();

            if (AllCardsMatched())
            {
                Victory();
                yield break;
            }
        }
        else
        {
            currentLives--;
            uiManager.UpdateLives(currentLives);

            UpdateSoundtrack(currentLives);

            yield return StartCoroutine(firstSelectedCard.Unflip());
            yield return StartCoroutine(secondSelectedCard.Unflip());

            if (currentLives <= 0)
            {
                yield return new WaitForSeconds(0.5f);
                GameOver();
                yield break;
            }
        }

        firstSelectedCard = null;
        secondSelectedCard = null;
        canSelect = true;
    }

    /*Actualiza la cantidad de vidas si no se ha formado un par
     * Args:
     *  lives: cantidad de vidas actuales
     */
    public void UpdateLives(int lives)
    {
        if (uiManager != null)
            uiManager.UpdateLives(lives);

        if (specialModel != null)
        {
            if (lives == 3)
            {
                specialModel.SetActive(true); // por si quieres mostrarlo solo en estos casos
            }
            else if (lives == 1)
            {
                specialModel.SetActive(true);
            }
            else
            {
                specialModel.SetActive(false); // Si quieres ocultarlo en otros casos
            }
        }

        UpdateSoundtrack(lives);
    }

    /*Actualiza el soundtrack en función de las vidas actuales
     * Args:
     *  lives: cantidad de vidas actuales
     */
    public void UpdateSoundtrack(int lives)
    {
        AudioClip clipToPlay = null;

        if (lives >= 4)
            clipToPlay = fullLivesClip;
        else if (lives >= 2)
            clipToPlay = mediumLivesClip;
        else
            clipToPlay = lowLivesClip;

        // Cambia de clip solo si el clip debe ser diferente
        if (musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
            currentClip = clipToPlay;
            Debug.Log($"Cambiando soundtrack: {clipToPlay.name}");
        }
        else
        {
            Debug.Log($"No se cambia soundtrack, ya está sonando: {clipToPlay.name}");
        }
    }

    /*Actualiza la cantidad de vidas si no se ha formado un par
     * Args:
     *  lives: cantidad de vidas actuales
     * Returns:
     *  bool: si se han encontrado todos los pares
     */
    private bool AllCardsMatched()
    {
        Card[] allCards = FindObjectsOfType<Card>();
        foreach (var card in allCards)
        {
            if (!card.IsMatched)
                return false;
        }
        return true;
    }

    //Se activa la condición de victoria
    private void Victory()
    {
        canSelect = false;
        musicSource.clip = winClip;
        musicSource.Play();
        uiManager.ShowWinPanel();
    }
    
    //Se activa la condición de derrota
    private void GameOver()
    {
        canSelect = false;
        marcador.SetActive(false);
        StartCoroutine(LoseSequence());
    }

    //Ejecuta la secuencia de ostrar la animación y la interfaz gráfica cuando se pierde
    private IEnumerator LoseSequence()
    {
        if (specialModel != null)
        {
            specialModel.SetActive(true);
        }

        //Activa el modelo 3D
        if (loseModel != null)
            loseModel.SetActive(true);

        //Ejecuta la animación
        if (loseAnimator != null && !string.IsNullOrEmpty(loseAnimationName))
            loseAnimator.Play(loseAnimationName);

        //Reproduce el audio de derrota
        if (loseAudioSource != null && loseClip != null)
        {
            musicSource.clip = loseClip;
            loseAudioSource.PlayDelayed(3.0f);
        }

        //Espera a que termine la animación
        yield return new WaitForSeconds(3.0f);

        //Desactiva el modelo
        if (loseModel != null)
            loseModel.SetActive(false);

        //Muestra la pantalla de Game Over
        if (uiManager != null)
            uiManager.ShowGameOverPanel();
    }

    //Indica el comportamiento para reiniciar el juego
    public void RestartGame()
    {
        currentLives = maxLives;
        uiManager.UpdateLives(currentLives);
        uiManager.HideGameOverPanel();
        canSelect = true;

        Card[] allCards = FindObjectsOfType<Card>();
        foreach (var card in allCards)
        {
            card.StartCoroutine(card.Unflip());
        }
    }
}

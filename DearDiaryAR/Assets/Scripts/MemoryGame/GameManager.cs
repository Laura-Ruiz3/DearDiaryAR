using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Vuforia;

public class GameManager : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public UIManager uiManager;

    private Card firstSelectedCard = null;
    private Card secondSelectedCard = null;
    private bool canSelect = true;

    public GameObject marcador;

    public AudioSource musicSource;
    public AudioClip fullLivesClip;
    public AudioClip mediumLivesClip;
    public AudioClip lowLivesClip;
    public AudioClip loseClip;
    public AudioClip winClip;

    //public GameObject loseImagePanel;
    //public float loseImageDuration = 2f; // tiempo que estará visible la imagen

    public GameObject loseModel;
    public Animator loseAnimator;
    public string loseAnimationName = "Lose"; // El nombre de la animación en tu Animator
    public AudioSource loseAudioSource;
    //public AudioClip loseClip;
    public float loseAnimationDuration = 10f; // Duración real de tu animación

    public GameObject specialModel; // El modelo que vas a mover

    // Define las posiciones para cada estado
    public Vector3 positionThreeLives = new Vector3(0, 0, 0);    // Posición cuando tiene 3 vidas
    public Vector3 positionOneLife = new Vector3(1, 0, 0);    // Posición cuando tiene 1 vida
    public Vector3 positionLose = new Vector3(0, 0, 1);    // Posición cuando pierde


    // Opcional: para evitar que la música reinicie al cambiar a la misma
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
            Debug.Log("Debería sonar el audio inicial");
        }
    }

    void Update()
    {
        if (!canSelect) return; // <-- AGREGADO: bloquea input si está false

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            DetectTouch(Input.mousePosition);
        }
#endif

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            DetectTouch(Input.GetTouch(0).position);
        }
    }

    //void DetectTouch(Vector2 screenPos)
    //{
    //    Camera arCamera = Camera.main;
    //    if (arCamera == null)
    //    {
    //        Debug.LogWarning("No se encontró la cámara principal para el Raycast.");
    //        return;
    //    }

    //    Ray ray = arCamera.ScreenPointToRay(screenPos);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, 100f))
    //    {
    //        Debug.Log($"TOUCH! Raycast hit: {hit.collider.name}");

    //        Card card = hit.collider.GetComponentInParent<Card>();
    //        if (card != null)
    //        {
    //            Debug.Log("Llamando a onSelected");
    //            card.OnSelected();
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("Raycast no tocó nada.");
    //    }
    //}    

void DetectTouch(Vector2 screenPos)
{
    // ---- BLOQUEO DE RAYCAST SI ES SOBRE UI ----
#if UNITY_EDITOR
    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        return;
#else
    if (EventSystem.current != null && Input.touchCount > 0)
    {
        int fingerId = Input.GetTouch(0).fingerId;
        if (EventSystem.current.IsPointerOverGameObject(fingerId))
            return;
    }
#endif
    // ---- FIN BLOQUEO DE RAYCAST SI ES SOBRE UI ----

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
        Debug.Log($"TOUCH! Raycast hit: {hit.collider.name}");

        Card card = hit.collider.GetComponentInParent<Card>();
        if (card != null)
        {
            Debug.Log("Llamando a onSelected");
            card.OnSelected();
        }
    }
    else
    {
        Debug.Log("Raycast no tocó nada.");
    }
}


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

            //StartCoroutine(firstSelectedCard.Unflip());
            //StartCoroutine(secondSelectedCard.Unflip());

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

    public void UpdateLives(int lives)
    {
        if (uiManager != null)
            uiManager.UpdateLives(lives);

        if (specialModel != null)
        {
            if (lives == 3)
            {
                specialModel.transform.localPosition = positionThreeLives;
                specialModel.SetActive(true); // por si quieres mostrarlo solo en estos casos
            }
            else if (lives == 1)
            {
                specialModel.transform.localPosition = positionOneLife;
                specialModel.SetActive(true);
            }
            else
            {
                specialModel.SetActive(false); // Si quieres ocultarlo en otros casos
            }
        }

        UpdateSoundtrack(lives);
    }

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


    private bool AllCardsMatched()
    {
        Card[] allCards = FindObjectsOfType<Card>();
        foreach (var card in allCards)
        {
            if (!card.IsMatched) // Puedes agregar la propiedad IsMatched
                return false;
        }
        return true;
    }

    private void Victory()
    {
        canSelect = false;
        musicSource.clip = winClip;
        musicSource.Play();
        uiManager.ShowWinPanel();
    }

    //private void GameOver()
    //{
    //    canSelect = false;
    //    uiManager.ShowGameOverPanel();

    //    marcador.SetActive(false);
    //}

    private void GameOver()
    {
        canSelect = false;
        marcador.SetActive(false);
        StartCoroutine(LoseSequence());
    }

    //private IEnumerator LoseSequence()
    //{
    //    // 1. Muestra imagen de derrota
    //    if (loseImagePanel != null)
    //        loseImagePanel.SetActive(true);

    //    // 2. Reproduce el audio de derrota
    //    if (musicSource != null && loseClip != null)
    //    {
    //        musicSource.clip = loseClip;
    //        musicSource.Play();
    //    }

    //    // 3. Espera N segundos
    //    yield return new WaitForSeconds(loseImageDuration);

    //    // 4. Oculta la imagen de derrota
    //    if (loseImagePanel != null)
    //        loseImagePanel.SetActive(false);

    //    // 5. Muestra pantalla de Game Over
    //    if (uiManager != null)
    //        uiManager.ShowGameOverPanel();
    //}

    private IEnumerator LoseSequence()
    {
        if (specialModel != null)
        {
            specialModel.transform.localPosition = positionLose;
            specialModel.SetActive(true);
        }

        // 1. Activa el modelo 3D
        if (loseModel != null)
            loseModel.SetActive(true);

        // 2. Ejecuta la animación
        if (loseAnimator != null && !string.IsNullOrEmpty(loseAnimationName))
            loseAnimator.Play(loseAnimationName);

        // 3. Reproduce el audio de derrota
        if (loseAudioSource != null && loseClip != null)
        {
            musicSource.clip = loseClip;
            loseAudioSource.PlayDelayed(3.0f);
        }

        // 4. Espera a que termine la animación
        yield return new WaitForSeconds(3.0f);

        // 5. Desactiva el modelo
        if (loseModel != null)
            loseModel.SetActive(false);

        // 6. Muestra Game Over panel
        if (uiManager != null)
            uiManager.ShowGameOverPanel();
    }


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
            // Reset matched state (opcional si quieres reutilizar las cartas)
            // card.isMatched = false;
        }
    }
}

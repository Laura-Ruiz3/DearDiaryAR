using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Vuforia;

//Se encarga de toda la lógica del juego (ejecución y condiciones de victoria y derrota)
public class ShuffleGameManager : MonoBehaviour
{
    //Cantidad de vidas
    public int maxLives = 3;
    private int currentLives;

    //Llamada a interfaz gráfica
    public ShuffleUIManager uiManager;

    //Vasos seleccionados
    private Cup selectedCup = null;
    private bool canSelect = true;

    //Marcador a detectar
    public GameObject marcador;

    //Audio del juego
    public AudioSource musicSource;    
    public AudioClip lowLivesClip;
    public AudioClip loseClip;
    public AudioClip winClip;
    private AudioClip currentClip;

    //Vasos y pelota para el juego
    public Cup[] cups;
    public GameObject ball;

    private int ballIndex;

    private const float cupLiftDuration = 0.7f; // Duración de la animación del vaso
    private Vector3 selectedCupOriginalLocalPosition;

    void Start()
    {
        currentLives = maxLives;
        uiManager.HideGameOverPanel();
        if (marcador != null && !marcador.activeSelf)
            marcador.SetActive(true);
        if (musicSource != null && lowLivesClip != null)
        {
            musicSource.clip = lowLivesClip;
            musicSource.Play();
        }
        SetupGame();
    }

    void Update()
    {
        if (!canSelect) return;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            DetectTouch(Input.GetTouch(0).position);
        }
    }

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
            Debug.Log($"TOUCH! Raycast hit: {hit.collider.name}");

            Cup cup = hit.collider.GetComponentInParent<Cup>();
            if (cup != null)
            {
                Debug.Log("Llamando a OnSelectedCup");
                OnCupSelected(cup);
            }
        }
        else
        {
            Debug.Log("Raycast no tocó nada.");
        }
    }

    //Inicia el minijuego de encontrar la pelota. Coloca los vasos y mezcla la pelota.
    void SetupGame()
    {
        ArrangeCups();

        ballIndex = UnityEngine.Random.Range(0, cups.Length);
        for (int i = 0; i < cups.Length; i++)
        {
            cups[i].Setup(this);
        }

        ball.SetActive(false);

        canSelect = true;
        selectedCup = null;
    }

    //Comportamiento cuando se selecciona un vaso
    public void OnCupSelected(Cup cup)
    {
        if (!canSelect || cup == selectedCup)
            return;

        selectedCup = cup;
        selectedCupOriginalLocalPosition = cup.transform.localPosition;
        canSelect = false;
        StartCoroutine(CheckCup());
    }

    //Mezcla los vasos
    private void ArrangeCups()
    {
        float separation = 0.15f;
        int numCups = cups.Length;

        float startX = -separation * (numCups - 1) / 2f;

        for (int i = 0; i < numCups; i++)
        {
            Vector3 pos = new Vector3(startX + i * separation, 0, 0);
            cups[i].transform.localPosition = pos;
            Debug.Log($"Vaso {i}: {pos}");
        }

    }

    /*Verifica si la bola está debajo del vaso.
     * Si se encuentra la pelota, va a condición de derrota.
     * Si no se encuentra, va a condición de victoria*/
    private IEnumerator CheckCup()
    {
        selectedCup.AnimateLift();
        yield return new WaitForSeconds(cupLiftDuration + 0.1f);

        int cupIndex = System.Array.IndexOf(cups, selectedCup);

        if (cupIndex == ballIndex)
        {
            Vector3 cupPos = cups[cupIndex].transform.position;
            float gap = 0.03f;
            float cupHeight = cups[cupIndex].GetComponent<Renderer>().bounds.size.y;
            float ballHeight = ball.GetComponent<Renderer>().bounds.size.y;
            Vector3 cupLocalPos = cups[cupIndex].transform.localPosition;
            ball.transform.localPosition = selectedCupOriginalLocalPosition;
            ball.SetActive(true);
            yield return new WaitForSeconds(1f);

            ball.SetActive(false);
            GameOver();
        }
        else
        {
            currentLives--;

            yield return new WaitForSeconds(1f);

            if (currentLives <= 0)
            {
                Victory();
                yield break;
            }
            else
            {
                selectedCup = null;
                canSelect = true;
            }
        }
    }

    //Condición de victoria
    private void Victory()
    {
        canSelect = false;
        musicSource.clip = winClip;
        musicSource.Play();
        canSelect = false;
        uiManager.ShowWinPanel();
    }

    //Condición de derrota
    private void GameOver()
    {
        if (loseClip != null && loseClip != null)
        {
            musicSource.clip = loseClip;
            musicSource.Play();
        }
        
        canSelect = false;
        uiManager.ShowGameOverPanel();
        if (marcador != null)
            marcador.SetActive(false);
    }

    //Reinicio del juego
    public void RestartGame()
    {
        currentLives = maxLives;
        uiManager.HideGameOverPanel();
        if (marcador != null && !marcador.activeSelf)
            marcador.SetActive(true);
        canSelect = true;
        SetupGame();
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Vuforia;

public class ShuffleGameManager : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public ShuffleUIManager uiManager;

    private Cup selectedCup = null;
    private bool canSelect = true;

    public GameObject marcador;

    public AudioSource musicSource;    
    public AudioClip lowLivesClip;
    public AudioClip loseClip;
    public AudioClip winClip;

    private AudioClip currentClip;

    public Cup[] cups;
    public GameObject ball;

    private int ballIndex;

    private const float cupLiftDuration = 0.7f; // Pon el mismo valor que en CupAR
    private Vector3 selectedCupOriginalLocalPosition;

    void Start()
    {
        currentLives = maxLives;
        //uiManager.UpdateLives(currentLives);
        uiManager.HideGameOverPanel();
        if (marcador != null && !marcador.activeSelf)
            marcador.SetActive(true);
        if (musicSource != null && lowLivesClip != null)
        {
            musicSource.clip = lowLivesClip;
            musicSource.Play();
            Debug.Log("Debería sonar el audio inicial");
        }
        SetupGame();
    }

    void Update()
    {
        if (!canSelect) return;

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

    void DetectTouch(Vector2 screenPos)
    {
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

    void SetupGame()
    {
        ArrangeCups();

        // Mezcla y coloca la pelota
        ballIndex = UnityEngine.Random.Range(0, cups.Length);
        for (int i = 0; i < cups.Length; i++)
        {
            cups[i].Setup(this);
            //cups[i].ResetCup();
        }
        //ball.transform.position = cups[ballIndex].transform.position + new Vector3(0, -0.08f, 0);

        ball.SetActive(false);

        Debug.Log("==== NUEVO JUEGO DE ENCONTRAR LA PELOTA ====");
        Debug.Log($"La pelota está bajo el vaso {ballIndex}");
        canSelect = true;
        selectedCup = null;
    }

    public void OnCupSelected(Cup cup)
    {
        if (!canSelect || cup == selectedCup)
            return;

        selectedCup = cup;
        // Guarda la posición original ANTES de animar el vaso
        selectedCupOriginalLocalPosition = cup.transform.localPosition;
        canSelect = false;
        StartCoroutine(CheckCup());
    }

    private void ArrangeCups()
    {
        float separation = 0.15f; // Ajusta este valor a tu gusto
        int numCups = cups.Length;

        float startX = -separation * (numCups - 1) / 2f;

        for (int i = 0; i < numCups; i++)
        {
            Vector3 pos = new Vector3(startX + i * separation, 0, 0);
            cups[i].transform.localPosition = pos;
            Debug.Log($"Vaso {i}: {pos}");
        }

    }

    private IEnumerator CheckCup()
    {
        selectedCup.AnimateLift();
        yield return new WaitForSeconds(cupLiftDuration + 0.1f);

        int cupIndex = System.Array.IndexOf(cups, selectedCup);

        if (cupIndex == ballIndex)
        {
            // ======= POSICIONA LA PELOTA JUSTO DEBAJO DEL VASO ========
            //float cupHeight = cups[cupIndex].GetComponent<Renderer>().bounds.size.y;
            //float ballHeight = ball.GetComponent<Renderer>().bounds.size.y;
            Vector3 cupPos = cups[cupIndex].transform.position;
            float gap = 0.03f;
            float cupHeight = cups[cupIndex].GetComponent<Renderer>().bounds.size.y;
            float ballHeight = ball.GetComponent<Renderer>().bounds.size.y;
            Vector3 cupLocalPos = cups[cupIndex].transform.localPosition;
            /*ball.transform.localPosition = new Vector3(
                cupLocalPos.x,
                cupLocalPos.y - (cupHeight / 2) - (ballHeight / 2) - gap,
                cupLocalPos.z
            );*/

            ball.transform.localPosition = selectedCupOriginalLocalPosition;
            ball.SetActive(true);
            // =========================================================

            Debug.Log($"¡Acierto! La pelota estaba en el vaso {cupIndex}");

            // Espera 1 segundo mostrando la pelota
            yield return new WaitForSeconds(1f);

            ball.SetActive(false);
            GameOver();
        }
        else
        {
            // Ganó
            currentLives--;
            //uiManager.UpdateLives(currentLives);

            Debug.Log($"Error. La pelota estaba en el vaso {ballIndex}, se eligió el vaso {cupIndex}");
            //UpdateSoundtrack(currentLives);

            yield return new WaitForSeconds(1f);

            if (currentLives <= 0)
            {
                Victory();
                yield break;
            }
            else
            {
                // Permitir siguiente intento
                selectedCup = null;
                canSelect = true;
            }
        }
    }


    //public void UpdateSoundtrack(int lives)
    //{
    //    AudioClip clipToPlay = null;

    //    if (lives >= 3)
    //        clipToPlay = fullLivesClip;
    //    else if (lives >= 2)
    //        clipToPlay = mediumLivesClip;
    //    else
    //        clipToPlay = lowLivesClip;

    //    if (musicSource != null && clipToPlay != null && musicSource.clip != clipToPlay)
    //    {
    //        musicSource.clip = clipToPlay;
    //        musicSource.Play();
    //        currentClip = clipToPlay;
    //        Debug.Log($"Cambiando soundtrack: {clipToPlay.name}");
    //    }
    //    else if (clipToPlay != null)
    //    {
    //        Debug.Log($"No se cambia soundtrack, ya está sonando: {clipToPlay.name}");
    //    }
    //}

    private void Victory()
    {
        canSelect = false;
        musicSource.clip = winClip;
        musicSource.Play();
        canSelect = false;
        uiManager.ShowWinPanel();
    }

    private void GameOver()
    {
        if (loseClip != null && loseClip != null)
        {
            musicSource.clip = loseClip;
            //musicSource.PlayDelayed(3.0f);
            musicSource.Play();
        }
        
        canSelect = false;
        uiManager.ShowGameOverPanel();
        if (marcador != null)
            marcador.SetActive(false);
    }

    public void RestartGame()
    {
        currentLives = maxLives;
        //uiManager.UpdateLives(currentLives);
        uiManager.HideGameOverPanel();
        if (marcador != null && !marcador.activeSelf)
            marcador.SetActive(true);
        canSelect = true;
        SetupGame();
        //UpdateSoundtrack(currentLives);
    }
}

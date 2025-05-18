using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class TextBoxMangment : MonoBehaviour
{
    [SerializeField] private GameObject[] continueModels;

    [SerializeField] private Transform textBoxItem; //Contiene todos los textos a mostrar
    [SerializeField] private TMP_Text textTemplate; //Texto a modificar
    [SerializeField] private Button skipButton; //Botón para saltar
    [SerializeField] private Button continueButton; //Botón para continuar
    public string sceneName; //Nombre de la escena a la que avanzará
    [SerializeField] private GameObject textBox; //Caja de texto
    [SerializeField] private GameObject backgroundPanel; //Fondo
    [SerializeField] private AudioSource audioSource; //Componente de audio
    [SerializeField] private AudioClip[] clips; //Audios a reproducir

    //Componentes visuales del diálogo
    [SerializeField] private GameObject[] icons;
    [SerializeField] private Image tempIcon;
    [SerializeField] private TMP_Text erasedTextTemplate;
    [SerializeField] private TMP_Text creepyTextTemplate;
    [SerializeField] private Image tempColor;

    [SerializeField] private float delay = 0.05f; //Delay al mostrar caracteres

    public bool markerDetected = false; //Estado del marcador (detectado o perdido)

    private bool[] targetFinished; // Determina si un marcador se detectó o no

    // Maneja la interrupción de la corrutina
    private bool stopTextCoroutine = false;
    private Coroutine currentTextRoutine;
    private bool[] alreadyShown;
    private int lastNoTarget = -1;

    void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(StopText);
        else
            Debug.LogWarning("No se asignó botón de skip");
        if (continueButton != null)
            continueButton.onClick.AddListener(GoToNextScene);
        else
            Debug.LogWarning("No se asignó botón de continuar");
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false); //Oculta el fondo del diálogo

        // Desactiva botones para saltar y continuar
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }

    void Start()
    {
        targetFinished = new bool[textBoxItem.childCount];
        int totalTargets = textBoxItem.childCount;
        alreadyShown = new bool[totalTargets];
        ShowText(-1); // Mostrar pantalla inicial (default)
    }

    public bool IsContinueActive()
    {
        return continueButton != null && continueButton.gameObject.activeSelf;
    }


    /*
     * Interrumpe la corrutina del texto. Oculta los componentes gráficos del diálogo.
     * Args:
     *  noTarget: marcador que está detectando
     */
    public void ShowText(int noTarget)
    {
        stopTextCoroutine = true;
        if (currentTextRoutine != null)
            StopCoroutine(currentTextRoutine);

        lastNoTarget = noTarget;

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        if (noTarget == -1)
        {
            if (backgroundPanel != null) 
                backgroundPanel.SetActive(false);
            if (textBox != null) 
                textBox.SetActive(true);
            currentTextRoutine = StartCoroutine(ShowTextCoroutine(noTarget));
            return;
        }

        // Si ya terminó/interrumpió la narración
        if (targetFinished != null && noTarget < targetFinished.Length && targetFinished[noTarget])
        {
            if (backgroundPanel != null) 
                backgroundPanel.SetActive(false);
            if (textBox != null) 
                textBox.SetActive(true);
            // Muestra el botón de Continuar si ya se había reproducido la narración
            if (continueButton != null)
                continueButton.gameObject.SetActive(markerDetected);
            return;
        }

        if (backgroundPanel != null)
            backgroundPanel.SetActive(true);
        if (skipButton != null)
            skipButton.gameObject.SetActive(true);
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
        if (textBox != null)
            textBox.SetActive(true);

        currentTextRoutine = StartCoroutine(ShowTextCoroutine(noTarget));
    }

    //Detecta si hay un marcador activo
    public bool IsTargetCoroutineRunning()
    {
        return currentTextRoutine != null && lastNoTarget >= 0;
    }

    //Maneja el caso en que se debe mostrar sólo el botón de Continuar.
    private void ShowOnlyContinue()
    {
        // Limpia los elementos gráficos
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
        if (textBox != null)
            textBox.SetActive(false);
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        if (textTemplate != null)
            textTemplate.text = string.Empty;
        if (erasedTextTemplate != null)
            erasedTextTemplate.text = string.Empty;
        if (creepyTextTemplate != null)
            creepyTextTemplate.text = string.Empty;
        if (tempColor != null)
            tempColor.color = Color.white;
        if (tempIcon != null)
            tempIcon.color = Color.white;
        if (icons != null)
            foreach (var icon in icons)
                if (icon != null) icon.SetActive(false);

        if (continueButton != null)
            continueButton.gameObject.SetActive(markerDetected);
        if (markerDetected && continueModels != null && lastNoTarget >= 0 && lastNoTarget < continueModels.Length)
            continueModels[lastNoTarget].SetActive(true);
    }



    /*
     * Reproduce la corrutina del texto. Muestra los componentes gráficos del diálogo.
     * Args:
     *  noTarget: marcador que está detectando
     */
    private IEnumerator ShowTextCoroutine(int noTarget)
    {
        stopTextCoroutine = false;
        textBox.SetActive(true);
        if (noTarget >= 0 && backgroundPanel != null)
            backgroundPanel.SetActive(true);

        textTemplate.text = string.Empty;
        foreach (var icon in icons)
            icon.SetActive(false);

        string fullText;
        string full2;

        // Lógica de textos, dependiendo del marcador detectado.
        switch (noTarget)
        {
            case 0:
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(true);
                textTemplate.text = "";
                yield return new WaitForSeconds(0.5f);

                audioSource.clip = clips[1];
                fullText = textBoxItem.Find("01")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);


                icons[0].SetActive(true);
                fullText = textBoxItem.Find("02")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                audioSource.clip = clips[2];
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("03")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("04")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);


                fullText = textBoxItem.Find("05")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[0].SetActive(false);
                icons[4].SetActive(true);
                fullText = textBoxItem.Find("06")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[4].SetActive(false);
                icons[0].SetActive(true);
                fullText = textBoxItem.Find("07")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[0].SetActive(false);
                icons[4].SetActive(true);
                fullText = textBoxItem.Find("08")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[4].SetActive(false);
                icons[0].SetActive(true);
                fullText = textBoxItem.Find("09")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);


                fullText = textBoxItem.Find("10")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("11")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay/0.9f);
                }
                yield return new WaitForSeconds(1.0f);
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(false);
                icons[0].SetActive(false);
                textTemplate.text = "";
                break;

            case 1:
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(true);
                textTemplate.text = "";
                yield return new WaitForSeconds(0.5f);

                audioSource.clip = clips[1];
                fullText = textBoxItem.Find("2_01")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);


                icons[9].SetActive(true);
                fullText = textBoxItem.Find("2_02")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                audioSource.clip = clips[2];
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[9].SetActive(false);
                icons[2].SetActive(true);
                fullText = textBoxItem.Find("2_03")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[2].SetActive(false);
                icons[4].SetActive(true);
                fullText = textBoxItem.Find("2_04")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[4].SetActive(false);
                icons[0].SetActive(true);
                fullText = textBoxItem.Find("2_05")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("2_06")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);

                }
                yield return new WaitForSeconds(1.0f);

                icons[0].SetActive(false);
                icons[9].SetActive(true);
                fullText = textBoxItem.Find("2_07")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                tempColor = textBox.GetComponent<Image>();
                tempColor.color = new Color(0.5f, 0.5f, 0.5f);
                tempIcon = icons[9].GetComponent<Image>();
                tempIcon.color = new Color(0.5f, 0.5f, 0.5f);
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    //audioSource.Play();
                    yield return new WaitForSeconds(delay*1.8f);
                }
                textTemplate.text = "";
                full2 = "DEBESCONTINUARCONTINUAJUGANDO";
                audioSource.clip = clips[3];
                foreach (char letter in full2)
                {
                    if (stopTextCoroutine)
                        yield break;
                    erasedTextTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay/3.0f);
                }
                icons[9].SetActive(false);
                tempColor.color = new Color(1.0f, 1.0f, 1.0f);
                tempIcon.color = new Color(1.0f, 1.0f, 1.0f);
                yield return new WaitForSeconds(0.3f);
                icons[12].SetActive(true);
                yield return new WaitForSeconds(0.25f);
                icons[12].SetActive(false);
                yield return new WaitForSeconds(0.5f);
                icons[4].SetActive(true);
                erasedTextTemplate.text = "";
                audioSource.clip = clips[2];
                fullText = textBoxItem.Find("2_08")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                Debug.Log("Se detuvo");
                yield return new WaitForSeconds(1.0f);
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(false);
                icons[4].SetActive(false);
                textTemplate.text = "";
                break;

            case 2:
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(true);
                textTemplate.text = "";
                yield return new WaitForSeconds(0.5f);

                audioSource.clip = clips[1];
                fullText = textBoxItem.Find("3_01")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[8].SetActive(true);
                fullText = textBoxItem.Find("3_02")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                audioSource.clip = clips[2];
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("3_03")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[8].SetActive(false);
                icons[2].SetActive(true);
                fullText = textBoxItem.Find("3_04")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[2].SetActive(false);
                icons[8].SetActive(true);
                fullText = textBoxItem.Find("3_05")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("3_06")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[8].SetActive(false);
                icons[6].SetActive(true);
                fullText = textBoxItem.Find("3_07")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                icons[6].SetActive(false);
                icons[8].SetActive(true);
                fullText = textBoxItem.Find("3_08")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);
                tempColor = textBox.GetComponent<Image>();
                tempColor.color = new Color(0.75f, 0.75f, 0.75f);
                tempIcon = icons[8].GetComponent<Image>();
                tempIcon.color = new Color(0.75f, 0.75f, 0.75f);
                fullText = textBoxItem.Find("3_09")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay*1.5f);
                }
                yield return new WaitForSeconds(1.0f);

                tempColor.color = new Color(0.50f, 0.50f, 0.50f);
                tempIcon.color = new Color(0.50f, 0.50f, 0.50f);
                fullText = textBoxItem.Find("3_10")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay*1.5f);
                }
                yield return new WaitForSeconds(1.0f);

                icons[8].SetActive(false);
                icons[7].SetActive(true);
                tempColor.color = new Color(0.25f, 0.25f, 0.25f);
                tempIcon = icons[7].GetComponent<Image>();
                tempIcon.color = new Color(0.25f, 0.25f, 0.25f);
                fullText = textBoxItem.Find("3_11")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay*1.5f);
                }
                textTemplate.text = "";
       
                tempColor.color = new Color(0.25f, 0.0f, 0.0f);
                tempIcon.color = new Color(0.25f, 0.0f, 0.0f);
                tempIcon = icons[10].GetComponent<Image>();
                tempIcon.color = new Color(0.25f, 0.0f, 0.0f);
                fullText = textBoxItem.Find("3_12")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    yield return new WaitForSeconds(delay/1.5f);
                }
                full2 = "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
    "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
    "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
    "■■■■■■■■■■■■■■■";
                audioSource.clip = clips[3];
                foreach (char letter in full2)
                {
                    if (stopTextCoroutine)
                        yield break;
                    erasedTextTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay / 50.0f);
                }
                yield return new WaitForSeconds(1.0f);

                icons[7].SetActive(false);
                tempIcon = icons[7].GetComponent<Image>();
                tempColor.color = new Color(1.0f, 1.0f, 1.0f);

                erasedTextTemplate.text = "";
                fullText = textBoxItem.Find("3_13")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    icons[10].SetActive(true);
                    yield return new WaitForSeconds(delay);
                    icons[10].SetActive(false);
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);
                foreach (char letter in full2)
                {
                    if (stopTextCoroutine)
                        yield break;
                    erasedTextTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay / 50.0f);
                }

                yield return new WaitForSeconds(0.5f);
                tempIcon = icons[14].GetComponent<Image>();
                tempIcon.color = new Color(0.25f, 0.0f, 0.0f);
                icons[14].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                icons[14].SetActive(false);
                yield return new WaitForSeconds(3.0f);
                erasedTextTemplate.text = "";
                icons[3].SetActive(true);

                tempColor.color = new Color(1.0f, 1.0f, 1.0f);
                tempIcon.color = new Color(1.0f, 1.0f, 1.0f);

                fullText = textBoxItem.Find("3_14")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("3_15")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                fullText = textBoxItem.Find("3_16")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);

                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(false);
                icons[3].SetActive(false);
                textTemplate.text = "";
                break;
            case 3:
                audioSource.clip = clips[0];
                audioSource.Play();
                yield return new WaitForSeconds(0.5f);
                tempIcon = icons[7].GetComponent<Image>();
                tempIcon.color = new Color(0.5f, 0.5f, 0.5f);
                tempIcon = icons[3].GetComponent<Image>();
                tempIcon.color = new Color(1.0f, 0.0f, 0.0f);
                tempIcon = icons[1].GetComponent<Image>();
                tempIcon.color = new Color(1.0f, 0.0f, 0.0f);
                tempColor = textBox.GetComponent<Image>();
                tempColor.color = new Color(0.5f, 0.5f, 0.5f);
                icons[7].SetActive(true);
                textBox.SetActive(true);
                textTemplate.text = "";
                audioSource.clip = clips[2];
                fullText = textBoxItem.Find("4_01")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay/1.5f);
                }
                TextShakeEffect a = textTemplate.GetComponent<TextShakeEffect>();
                a.enabled = true;
                full2 = "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■";
                audioSource.clip = clips[3];
                foreach (char letter in full2)
                {
                    if (stopTextCoroutine)
                        yield break;
                    erasedTextTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay / 50.0f);
                }
                yield return new WaitForSeconds(1.0f);
                erasedTextTemplate.text = "";
                a.enabled=false;

                tempColor.color = new Color(1.0f, 0.0f, 0.0f);
                icons[7].SetActive(false);
                icons[3].SetActive(true);
                fullText = textBoxItem.Find("4_02")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    //audioSource.Play();
                    yield return new WaitForSeconds(delay*2.0f);
                }
                yield return new WaitForSeconds(1.0f);

                tempColor.color = new Color(0.5f, 0.5f, 0.5f);
                icons[7].SetActive(true);
                icons[3].SetActive(false);
                fullText = textBoxItem.Find("4_03")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                audioSource.clip = clips[2];
                
                
                foreach (char letter in fullText)
                {
                    
                    if (stopTextCoroutine)
                        yield break;
                    textTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay/1.5f);
                }
                a.enabled = true;
                yield return new WaitForSeconds(0.5f);
                a.enabled = false;
                full2 = "■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■" +
"■■■■■■■■■■■■■■■";
                audioSource.clip = clips[3];
                foreach (char letter in full2)
                {
                    if (stopTextCoroutine)
                        yield break;
                    erasedTextTemplate.text += letter;
                    audioSource.Play();
                    yield return new WaitForSeconds(delay / 50.0f);
                }
                yield return new WaitForSeconds(1.0f);
                erasedTextTemplate.text = "";

                tempColor.color = new Color(1.0f, 0.0f, 0.0f);
                icons[7].SetActive(false);
                icons[1].SetActive(true);
                fullText = textBoxItem.Find("4_04")?.GetComponent<TMP_Text>().text;
                textTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.15f);
                }
                yield return new WaitForSeconds(1.0f);
                audioSource.clip = clips[0];
                audioSource.Play();
                textBox.SetActive(false);
                icons[1].SetActive(false);
                creepyTextTemplate.text = "";
                break;
            case 4:
                icons[5].SetActive(true);
                tempColor = textBox.GetComponent<Image>();
                tempColor.color = new Color(0.0f, 0.0f, 0.0f);
                textBox.SetActive(true);
                yield return new WaitForSeconds(2.0f);

                creepyTextTemplate.text = "";
                creepyTextTemplate.color = new Color(1.0f, 1.0f, 1.0f);
                audioSource.clip = clips[4];
                fullText = textBoxItem.Find("5_01")?.GetComponent<TMP_Text>().text;

                
                audioSource.Play();
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.14f);
                }
                yield return new WaitForSeconds(0.15f);
                fullText = textBoxItem.Find("5_02")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.13f);
                }
                icons[5].SetActive(false);
                icons[11].SetActive(true);
                fullText = textBoxItem.Find("5_03")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.12f);
                }
                fullText = textBoxItem.Find("5_04")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.12f);
                }
                audioSource.clip = clips[5];
                textBox.SetActive(true);
                fullText = textBoxItem.Find("5_05")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                audioSource.Play();
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.13f);
                }
                fullText = textBoxItem.Find("5_06")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                foreach (char letter in fullText)
                {
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.14f);
                }
                fullText = textBoxItem.Find("5_07")?.GetComponent<TMP_Text>().text;
                creepyTextTemplate.text = "";
                foreach (char letter in fullText)
                {
                    icons[13].SetActive(true);
                    if (stopTextCoroutine)
                        yield break;
                    creepyTextTemplate.text += letter;
                    yield return new WaitForSeconds(0.07f);
                    icons[13].SetActive(false);
                    yield return new WaitForSeconds(0.07f);
                }
                icons[11].SetActive(false);
                icons[13].SetActive(true);
                yield return new WaitForSeconds(1.0f);
                textBox.SetActive(false);
                icons[13].SetActive(false);
                creepyTextTemplate.text = "";
                break;
            default:
                tempColor = textBox.GetComponent<Image>();
                tempColor.color = new Color(1.0f, 1.0f, 1.0f);
                textBox.SetActive(true);
                Debug.LogWarning("Entró a Default");
                fullText = textBoxItem.Find("FindPage")?.GetComponent<TMP_Text>()?.text;
                textTemplate.text = string.Empty;
                foreach (char c in fullText)
                {
                    if (stopTextCoroutine) yield break;
                    textTemplate.text += c;
                    yield return new WaitForSeconds(delay);
                }
                yield return new WaitForSeconds(1.0f);
                break;
        }

        yield return new WaitForSeconds(0.5f);

        if (noTarget >= 0)
        {
            targetFinished[noTarget] = true;
            ShowOnlyContinue(); // Limpia todo y deja solo Continue
            yield break;
        }
        else
        {
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }
    }

    //Detiene la reproducción del texto.
    public void StopText()
    {
        if (!stopTextCoroutine)
        {
            stopTextCoroutine = true;
            if (currentTextRoutine != null)
                StopCoroutine(currentTextRoutine);

            CleanUpUI();

            // Muestra el botón Continue solo si el marcador sigue detectado
            if (continueButton != null)
                continueButton.gameObject.SetActive(markerDetected);

            // Marca el target como terminado
            if (lastNoTarget >= 0 && targetFinished != null && lastNoTarget < targetFinished.Length)
                targetFinished[lastNoTarget] = true;

            ShowOnlyContinue();
        }
    }


    //Cambia de escena cuando se presiona continuar
    public void GoToNextScene()
    {
        SceneManager.LoadScene(sceneName); // Cambia a tu escena objetivo real
    }

    //Limpia los elementos gráficos en pantalla y muestra la cámara.
    private void CleanUpUI()
    {
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
        if (textBox != null)
            textBox.SetActive(false);
        if (textBoxItem != null)
            textBoxItem.gameObject.SetActive(false);


        textTemplate.text = string.Empty;
        erasedTextTemplate.text = string.Empty;
        creepyTextTemplate.text = string.Empty;
        if (tempColor != null)
            tempColor.color = Color.white;
        if (tempIcon != null)
            tempIcon.color = Color.white;
        foreach (var icon in icons)
            icon.SetActive(false);

        if (skipButton != null)
            skipButton.gameObject.SetActive(false);

        stopTextCoroutine = false;
        currentTextRoutine = null;
    }
}
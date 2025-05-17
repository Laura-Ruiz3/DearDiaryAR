using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;
using static UnityEngine.GraphicsBuffer;
using Image = UnityEngine.UI.Image;

public class TextBoxMangment : MonoBehaviour
{
    [SerializeField] private GameObject[] continueModels;

    [Header("UI References")]
    [Tooltip("Container of pages with TMP_Text components (e.g. FindPage, 01, 02, etc.)")]
    [SerializeField] private Transform textBoxItem;

    [Tooltip("Text field where characters will be typed out")]
    [SerializeField] private TMP_Text textTemplate;

    [Tooltip("Button to skip the typing animation")]
    [SerializeField] private Button skipButton;

    [Tooltip("Button to continue (go to next scene)")]
    [SerializeField] private Button continueButton;
    public string sceneName;

    [Tooltip("Main text box GameObject to show/hide")]
    [SerializeField] private GameObject textBox;

    [Tooltip("Background panel behind the text box blocking AR interaction")]
    [SerializeField] private GameObject backgroundPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    [Header("Icons and Colors")]
    [SerializeField] private GameObject[] icons;
    [SerializeField] private Image tempIcon;
    [SerializeField] private TMP_Text erasedTextTemplate;
    [SerializeField] private TMP_Text creepyTextTemplate;
    [SerializeField] private Image tempColor;

    [Header("Typing Settings")]
    [SerializeField] private float delay = 0.05f;

    [HideInInspector] public bool markerDetected = false;

    private bool[] targetFinished; // true si ya terminó o fue interrumpida

    // Internal state
    private bool stopTextCoroutine = false;
    private Coroutine currentTextRoutine;
    private bool[] alreadyShown;
    private int lastNoTarget = -1;

    void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(StopText);
        else
            Debug.LogWarning("Skip Button not assigned in TextBoxMangment");
        if (continueButton != null)
            continueButton.onClick.AddListener(GoToNextScene);
        else
            Debug.LogWarning("Continue Button not assigned in TextBoxMangment");
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);

        // Ambos botones ocultos por defecto
        if (skipButton != null)
            skipButton.gameObject.SetActive(false);
        if (continueButton != null)
            continueButton.gameObject.SetActive(false);
    }

    //Start is called before the first frame update
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


    /// <summary>
    /// Called by Skip/Close button: stops typing and immediately hides UI.
    /// </summary>
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
            if (backgroundPanel != null) backgroundPanel.SetActive(false);
            if (textBox != null) textBox.SetActive(true);
            currentTextRoutine = StartCoroutine(ShowTextCoroutine(noTarget));
            return;
        }

        // Si ya terminó/interrumpió la animación de ese target
        if (targetFinished != null && noTarget < targetFinished.Length && targetFinished[noTarget])
        {
            if (backgroundPanel != null) backgroundPanel.SetActive(false);
            if (textBox != null) textBox.SetActive(true);
            // Continue solo si markerDetected == true
            if (continueButton != null)
                continueButton.gameObject.SetActive(markerDetected);
            return;
        }

        // Animación: fondo, skip sí, continue no
        if (backgroundPanel != null) backgroundPanel.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (textBox != null) textBox.SetActive(true);

        currentTextRoutine = StartCoroutine(ShowTextCoroutine(noTarget));
    }

    public bool IsTargetCoroutineRunning()
    {
        // Considera que el default es -1, cualquier otro valor es target válido
        return currentTextRoutine != null && lastNoTarget >= 0;
    }

    private void ShowOnlyContinue()
    {
        // Limpia la UI y deja solo Continue
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

        // Aquí activas el modelo correspondiente al último target, si el marcador está presente
        if (markerDetected && continueModels != null && lastNoTarget >= 0 && lastNoTarget < continueModels.Length)
            continueModels[lastNoTarget].SetActive(true);
    }



    /// <summary>
    /// Corrutina de mostrar texto con skip y continuar
    /// </summary>
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

        // --- BEGIN ORIGINAL ShowText LOGIC ---
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
                //tempIcon = icons[8].GetComponent<Image>();
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
                    //audioSource.Play();
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
                    //audioSource.Play();
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
                    //audioSource.Play();
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
                    //audioSource.Play();
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
                    //audioSource.Play();
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
                    //audioSource.Play();
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

        // FIN DE ANIMACIÓN: OCULTA SKIP, MUESTRA CONTINUE SÓLO PARA TARGET VÁLIDO
        yield return new WaitForSeconds(0.5f);

        //if (noTarget >= 0)
        //{
        //    targetFinished[noTarget] = true;
        //    if (backgroundPanel != null) 
        //        backgroundPanel.SetActive(false); // Quita fondo
        //    if (skipButton != null) 
        //        skipButton.gameObject.SetActive(false);
        //    if (continueButton != null) 
        //        continueButton.gameObject.SetActive(true);
        //    CleanUpUI();
        //}
        if (noTarget >= 0)
        {
            targetFinished[noTarget] = true;
            ShowOnlyContinue(); // Limpia todo y deja solo Continue
            yield break; // Opcional, si ya no hay nada más
        }
        else
        {
            if (skipButton != null)
                skipButton.gameObject.SetActive(false);
            if (continueButton != null)
                continueButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Detiene la animación y muestra el botón continuar si corresponde.
    /// </summary>
    //public void StopText()
    //{
    //    if (!stopTextCoroutine)
    //    {
    //        stopTextCoroutine = true;
    //        if (currentTextRoutine != null)
    //            StopCoroutine(currentTextRoutine);

    //        if (lastNoTarget >= 0)
    //        {
    //            // Si el marcador sigue detectado, muestra Continue y quita fondo
    //            if (markerDetected)
    //            {
    //                targetFinished[lastNoTarget] = true;
    //                if (backgroundPanel != null) backgroundPanel.SetActive(false);
    //                if (continueButton != null) continueButton.gameObject.SetActive(true);
    //                if (skipButton != null) skipButton.gameObject.SetActive(false);
    //            }
    //            else // Si ya NO está el marcador, vuelve a default
    //            {
    //                ShowText(-1);
    //            }
    //        }
    //    }
    //}
    public void StopText()
    {
        if (!stopTextCoroutine)
        {
            stopTextCoroutine = true;
            if (currentTextRoutine != null)
                StopCoroutine(currentTextRoutine);

            // Limpia toda la UI y textos
            CleanUpUI();

            // Mostrar el botón Continue solo si el marcador sigue detectado
            if (continueButton != null)
                continueButton.gameObject.SetActive(markerDetected);

            // Marca el target como terminado
            if (lastNoTarget >= 0 && targetFinished != null && lastNoTarget < targetFinished.Length)
                targetFinished[lastNoTarget] = true;

            ShowOnlyContinue();
        }
    }


    /// <summary>
    /// Cambia de escena al presionar continuar.
    /// </summary>
    public void GoToNextScene()
    {
        SceneManager.LoadScene(sceneName); // Cambia a tu escena objetivo real
    }

    /// <summary>
    /// Oculta UI y resetea el estado interno para reactivar la AR camera.
    /// </summary>
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


    //// Update is called once per frame
    void Update()
    {

    }
}
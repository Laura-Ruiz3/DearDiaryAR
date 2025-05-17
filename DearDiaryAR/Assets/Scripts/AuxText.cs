using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class AuxText : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Container of pages with TMP_Text components (e.g. FindPage, 01, 02, etc.)")]
    [SerializeField] private Transform textBoxContainer;

    [Tooltip("Text field where characters will be typed out")]
    [SerializeField] private TMP_Text textTemplate;

    [Tooltip("Button to skip or close the text UI")]
    [SerializeField] private Button skipButton;

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

    // Internal state
    private bool stopCoroutineText = false;
    private Coroutine currentTextRoutine;
    private string lastFullText = string.Empty;

    void Awake()
    {
        if (skipButton != null)
            skipButton.onClick.AddListener(StopText);
        else
            Debug.LogWarning("Skip/Close Button not assigned in TextBoxMangment");

        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
    }

    void Start()
    {
        // Show initial default text at startup without background
        ShowText(-1);
    }

    /// <summary>
    /// Starts displaying text for a given target index, canceling any ongoing typing.
    /// </summary>
    public void ShowText(int noTarget)
    {
        // Cancel any ongoing typing and clean UI
        if (currentTextRoutine != null)
        {
            stopCoroutineText = true;
            StopCoroutine(currentTextRoutine);
            CleanUpUI();
        }

        // Begin new typing sequence
        currentTextRoutine = StartCoroutine(ShowTextRoutine(noTarget));
    }

    private IEnumerator ShowTextRoutine(int noTarget)
    {
        stopCoroutineText = false;

        // Show text UI
        textBox.SetActive(true);
        textTemplate.text = string.Empty;
        foreach (var icon in icons)
            icon.SetActive(false);

        // Show background only when a valid target is detected
        if (noTarget >= 0 && backgroundPanel != null)
            backgroundPanel.SetActive(true);

        // Select page based on target index
        Transform pageTransform = (noTarget >= 0)
            ? textBoxContainer.GetChild(noTarget)
            : textBoxContainer.Find("FindPage");

        if (pageTransform == null)
        {
            Debug.LogError($"Page not found for index {noTarget}");
            yield break;
        }

        TMP_Text pageComp = pageTransform.GetComponent<TMP_Text>();
        lastFullText = pageComp != null ? pageComp.text : string.Empty;

        // Play opening sound if any
        if (audioSource != null && clips.Length > 0)
        {
            audioSource.clip = clips[0];
            audioSource.Play();
        }

        // Type out characters until requested to stop
        foreach (char c in lastFullText)
        {
            if (stopCoroutineText)
                yield break;
            textTemplate.text += c;
            yield return new WaitForSeconds(delay);
        }

        // After typing completes, optionally wait then cleanup
        yield return new WaitForSeconds(1f);
        CleanUpUI();
    }

    /// <summary>
    /// Called by Skip/Close button to immediately hide text UI and unblock AR camera.
    /// </summary>
    public void StopText()
    {
        // Stop typing and remove UI
        if (currentTextRoutine != null && !stopCoroutineText)
        {
            stopCoroutineText = true;
            StopCoroutine(currentTextRoutine);
            CleanUpUI();
        }
    }

    /// <summary>
    /// Reset UI elements to hidden/cleared state and allow AR interaction again.
    /// </summary>
    private void CleanUpUI()
    {
        if (backgroundPanel != null)
            backgroundPanel.SetActive(false);
        if (textBox != null)
            textBox.SetActive(false);

        textTemplate.text = string.Empty;
        erasedTextTemplate.text = string.Empty;
        creepyTextTemplate.text = string.Empty;
        if (tempColor != null)
            tempColor.color = Color.white;
        if (tempIcon != null)
            tempIcon.color = Color.white;
        foreach (var icon in icons)
            icon.SetActive(false);

        currentTextRoutine = null;
    }
}

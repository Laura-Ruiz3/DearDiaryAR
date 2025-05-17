using System.Collections;
using TMPro;
using UnityEngine;

public class Narration : MonoBehaviour
{
    public TMP_Text textComponent;
    public float typingSpeed = 0.05f;
    public AudioClip typingSound;        
    public float pitchVariation = 0.1f;  

    private AudioSource audioSource;
    private string fullText;
    private string currentText = "";

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void StartTyping(string text)
    {
        fullText = text;
        currentText = "";
        StopAllCoroutines();
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        foreach (char letter in fullText.ToCharArray())
        {
            currentText += letter;
            textComponent.text = currentText;

            // Reproduce el sonido si es una letra visible (no espacio)
            if (typingSound && letter != ' ' && audioSource)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(typingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }
    }
}

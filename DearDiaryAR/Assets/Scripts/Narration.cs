using System.Collections;
using TMPro;
using UnityEngine;

public class Narration : MonoBehaviour
{
    public TMP_Text textComponent; //Componente de texto que se va a editar
    public float typingSpeed = 0.05f; //Velocidad que aparecen los caracteres
    public AudioClip typingSound; //Efecto de sonido por caracter
    public float pitchVariation = 0.1f; //Variacipon en el sonido

    private AudioSource audioSource; //Componente para reproducir audio
    private string fullText; //Cadena de texto completa a mostrar
    private string currentText = ""; //Texto actual en la caja de texto

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

    //Empieza a escribir el texto en la caja de texto
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

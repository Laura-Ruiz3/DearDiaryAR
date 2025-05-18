using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Fade : MonoBehaviour
{
    public static Fade Instance;

    public Image fadeImage; //Imagen que se le aplicará el efecto de fade
    public CanvasGroup uiCanvasGroup; //Canvas donde se encuentra la imagen
    public string sceneName; // Indica la siguiente escena a reproduci después del efecto
    public float fadeDuration = 1f; //Duración del efecto
    public AudioSource effect; //Efecto al pasar de escena

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        effect.Stop();
    }

    private void Start()
    {
        //Establece la imagen como opaca
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        StartCoroutine(FadeIn());
    }

    // Cambia la escena indicada en el inspector
    public void ChangeSceneFromInspector()
    {
        ChangeScene(sceneName);
    }

    //Hace el cambio de escena
    public void ChangeScene(string newSceneName)
    {
        effect.Play();
        Debug.Log("Cambiando a la escena: " + newSceneName);
        StartCoroutine(WaitThenFadeOut(newSceneName));
    }

    //Llama a la corrutina que hace el effecto de fade out
    IEnumerator WaitThenFadeOut(string sceneToLoad)
    {
        yield return StartCoroutine(FadeOut(sceneToLoad));
    }

    //Corrutina que hace efecto de fade in a la escena. Aumenta la opacidad de la imagen.
    IEnumerator FadeIn()
    {
        float t = 0;
        while (t > 0f)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }

            if (uiCanvasGroup != null)
                uiCanvasGroup.alpha = alpha;

            yield return null;
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
        }

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 1f;
    }

    /*Corrutina que hace efecto de fade out a la escena. Disminuye la opacidad de la imagen.
     * 
     *Args:
     *  sceneToLoad: escena a la cual cambiará
     */
    IEnumerator FadeOut(string sceneToLoad)
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1 - (t / fadeDuration);

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
            }

            if (uiCanvasGroup != null)
                uiCanvasGroup.alpha = alpha;

            yield return null;
        }

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        if (uiCanvasGroup != null)
            uiCanvasGroup.alpha = 0f;

        SceneManager.LoadScene(sceneToLoad);
    }
}

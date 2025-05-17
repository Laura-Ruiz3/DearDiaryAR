using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Fade : MonoBehaviour
{
    public static Fade Instance;

    public Image fadeImage;
    public CanvasGroup uiCanvasGroup;
    public string sceneName; // Ahora se puede asignar desde el Inspector
    public float fadeDuration = 1f;
    public AudioSource effect;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        effect.Stop();
    }

    private void Start()
    {
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        StartCoroutine(FadeIn());
    }

    // Nuevo método para cambiar a la escena indicada en el inspector
    public void ChangeSceneFromInspector()
    {
        ChangeScene(sceneName);
    }

    public void ChangeScene(string newSceneName)
    {
        effect.Play();
        Debug.Log("Cambiando a la escena: " + newSceneName);
        StartCoroutine(WaitThenFadeOut(newSceneName));
    }

    IEnumerator WaitThenFadeOut(string sceneToLoad)
    {
        yield return StartCoroutine(FadeOut(sceneToLoad));
    }

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

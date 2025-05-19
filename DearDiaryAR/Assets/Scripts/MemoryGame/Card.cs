using UnityEngine;
using System.Collections;

public class Card : MonoBehaviour
{
    public int cardId;
    public Material frontMaterial;
    public Material backMaterial;
    public MeshRenderer frontRenderer;
    public MeshRenderer backRenderer;

    private bool isFlipped = false;
    private bool isAnimating = false;
    private bool isMatched = false;

    private GameManager gameManager;
    public bool IsMatched => isMatched;


    void Start()
    {
        backRenderer.material = backMaterial;
        transform.rotation = Quaternion.Euler(-90, 0, 0);
        gameManager = FindAnyObjectByType<GameManager>();
    }

    //Detecta cuando la carta ha sido seleccionada
    public void OnSelected()
    {
        Debug.Log($"Carta seleccionada: {gameObject.name} | Par: {cardId}");

        if (isAnimating || isMatched || isFlipped) return;

        StartCoroutine(FlipCard());
        if (gameManager != null)
            gameManager.OnCardSelected(this);
    }

    //Realiza la animación de volteo de carts
    public IEnumerator FlipCard()
    {
        isAnimating = true;

        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);

        bool materialChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            if (t > 0.5f && !isFlipped && !materialChanged)
            {
                frontRenderer.material = frontMaterial;
                materialChanged = true;
            }
            yield return null;
        }

        transform.rotation = endRotation;
        isFlipped = !isFlipped;
        isAnimating = false;
    }

    //Indica cuando dos cartas son par o se emparejaron
    public void SetMatched()
    {
        isMatched = true;
    }

    //Voltea las cartas a su posición inicial en caso de que no sean par
    public IEnumerator Unflip()
    {
        if (isAnimating || !isFlipped) yield break;

        isAnimating = true;

        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 180, 0);

        bool materialChanged = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            if (t > 0.5f && isFlipped && !materialChanged)
            {
                frontRenderer.material = backMaterial;
                materialChanged = true;
            }
            yield return null;
        }

        transform.rotation = endRotation;
        isFlipped = !isFlipped;
        isAnimating = false;
    }
}

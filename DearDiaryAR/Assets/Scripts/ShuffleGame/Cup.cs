using UnityEngine;

public class Cup : MonoBehaviour
{
    private ShuffleGameManager gameManager;
    private Vector3 initialPosition;
    private bool isLifted = false;

    private float raiseHeight = 0.15f;
    private float animationDuration = 0.7f;

    private Vector3 initialLocalPosition;

    // Llamado por GameManagerAR al iniciar/reiniciar
    public void Setup(ShuffleGameManager manager)
    {
        gameManager = manager;
        initialLocalPosition = transform.localPosition;
        isLifted = false;
        transform.localPosition = initialLocalPosition;
    }

    // Animar levantamiento del vaso (coroutine para suavidad)
    public void AnimateLift()
    {
        if (!isLifted)
            StartCoroutine(RaiseCup());
    }

    private System.Collections.IEnumerator RaiseCup()
    {
        isLifted = true;
        Vector3 targetPos = initialPosition + Vector3.up * raiseHeight;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            transform.localPosition = Vector3.Lerp(initialLocalPosition, targetPos, elapsed / animationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;
        Debug.Log($"{name}: Animación de levantamiento completada.");
    }

    // Regresa el vaso a su posición inicial para reiniciar el juego
    public void ResetCup()
    {
        StopAllCoroutines();
        transform.localPosition = initialPosition;
        isLifted = false;
        Debug.Log($"{name}: ResetCup - Vaso en posición inicial.");
    }
}

using UnityEngine;

public class ShuffleManager : MonoBehaviour
{
    public GameObject[] cups; // Vasos
    public GameObject ball;
    private int ballIndex;

    void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        // Oculta la pelota debajo de un vaso al azar
        ballIndex = Random.Range(0, cups.Length);
        for (int i = 0; i < cups.Length; i++)
        {
            cups[i].transform.position = new Vector3(i * 0.15f, 0, 0); // Espaciado básico
            if (i == ballIndex)
                ball.transform.position = cups[i].transform.position + Vector3.up * 0.05f;
        }
        // Aquí puedes agregar animaciones de mezcla realistas
    }

    public void CheckCup(int index)
    {
        if (index == ballIndex)
        {
            Debug.Log("¡Correcto!");
            // Levanta el vaso y muestra la pelota
            // Puedes animar el vaso y la pelota aquí
        }
        else
        {
            Debug.Log("Incorrecto");
            // Levanta el vaso y no muestra pelota
        }
    }
}

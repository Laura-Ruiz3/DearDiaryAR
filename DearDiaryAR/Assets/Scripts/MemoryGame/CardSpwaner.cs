using UnityEngine;
using System.Collections.Generic;

//Script que se encarga de spawnear cartas dado un prefab de carta.

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab; //Prefab de la carta que va a spawnear
    public Transform parent; // ImageTarget o vacío sobre marcador
    public Material[] frontMaterials; // Imágenes de las cartas
    public Material backMaterial; //Material del reverso de la carta

    //Espacio entre cartas
    public float spacingX = 0.22f;
    public float spacingZ = 0.28f;

    void Start()
    {
        SpawnCards();
    }

    //Genera un par de cada diseño de carta
    void SpawnCards()
    {
        int rows = 2;
        int cols = 5;
        int totalCards = rows * cols;

        // Prepara los pares de materiales
        List<int> cardIds = new List<int>();
        for (int i = 0; i < frontMaterials.Length; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }
        Shuffle(cardIds);

        Vector3 startPos = new Vector3(
            -((cols - 1) * spacingX) / 2f,
            0f,
            -((rows - 1) * spacingZ) / 2f
        );

        int idx = 0;
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 localPos = startPos + new Vector3(col * spacingX, 0.05f, row * spacingZ);
                GameObject cardObj = Instantiate(cardPrefab, parent);
                cardObj.transform.localPosition = localPos;
                cardObj.transform.localRotation = Quaternion.Euler(90, 180, 0);

                Card card = cardObj.GetComponent<Card>();
                int cardId = cardIds[idx];

                card.cardId = cardId;
                card.frontMaterial = frontMaterials[cardId];
                card.backMaterial = backMaterial;
                idx++;
            }
        }
    }

    //Realiza la aparición aleatoria de las cartas.
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}

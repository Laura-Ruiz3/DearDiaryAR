using UnityEngine;
using System.Collections.Generic;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform parent; // ImageTarget o vacío sobre marcador
    public Material[] frontMaterials; // 5 materiales únicos, ¡verifica en el inspector!
    public Material backMaterial;

    public float spacingX = 0.22f;
    public float spacingZ = 0.28f;

    //public Vector3 cardScale = new Vector3(1f, 1f, 1f);


    void Start()
    {
        SpawnCards();
    }

    void SpawnCards()
    {
        int rows = 2;
        int cols = 5;
        int totalCards = rows * cols;

        // Prepara los pares de materiales (dos de cada uno)
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
                // Cambia la escala aquí (ajusta los valores según necesites)
                //cardObj.transform.localScale = cardScale;       
                cardObj.transform.localPosition = localPos;
                // IMPORTANTE: la rotación depende de cómo esté tu modelo. Ajusta si es necesario
                cardObj.transform.localRotation = Quaternion.Euler(90, 180, 0);

                Card card = cardObj.GetComponent<Card>();
                int cardId = cardIds[idx];

                card.cardId = cardId;
                card.frontMaterial = frontMaterials[cardId];
                card.backMaterial = backMaterial;
                //card.meshRenderer = cardObj.GetComponentInChildren<MeshRenderer>();

                // Debug: Para que veas en consola qué id y material tiene cada carta
                Debug.Log($"Carta {idx}: ID={cardId}, material={frontMaterials[cardId].name}");

                idx++;
            }
        }
    }

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

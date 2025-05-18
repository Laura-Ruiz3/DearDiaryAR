using UnityEngine;
using TMPro;

public class TextShakeEffect : MonoBehaviour
{
    public float shakeAmount = 1.0f; //Cantidad de shake que se va a aplicar
    public float shakeSpeed = 10.0f; //Velocidad del shake del texto

    private TMP_Text textComponent; //Componente de texto que se va a modificar
    private TMP_TextInfo textInfo; //Texto que se va a agregar
    private Vector3[][] originalVertices; //Vértices de cada caracter

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Start()
    {
        textComponent.ForceMeshUpdate();
        textInfo = textComponent.textInfo;
        originalVertices = new Vector3[textInfo.meshInfo.Length][];
    }

    void Update()
    {
        if (textComponent.text.Length>0){
            textComponent.ForceMeshUpdate();
            textInfo = textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                int meshIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                if (originalVertices[meshIndex] == null || originalVertices[meshIndex].Length < textInfo.meshInfo[meshIndex].vertices.Length)
                    originalVertices[meshIndex] = textInfo.meshInfo[meshIndex].vertices.Clone() as Vector3[];

                Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

                Vector3 offset = new Vector3(
                    Mathf.Sin(Time.time * shakeSpeed + i) * shakeAmount,
                    Mathf.Cos(Time.time * shakeSpeed + i) * shakeAmount,
                    0);

                vertices[vertexIndex + 0] = originalVertices[meshIndex][vertexIndex + 0] + offset;
                vertices[vertexIndex + 1] = originalVertices[meshIndex][vertexIndex + 1] + offset;
                vertices[vertexIndex + 2] = originalVertices[meshIndex][vertexIndex + 2] + offset;
                vertices[vertexIndex + 3] = originalVertices[meshIndex][vertexIndex + 3] + offset;
            }

            // Aplicar los cambios
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}

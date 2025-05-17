using UnityEngine;

public class ARTouchRaycast : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Crear rayo desde el toque
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Toque detectado en: " + hit.transform.name);

                // Aquí puedes hacer algo con el objeto tocado
                if (hit.transform.CompareTag("Interactuable"))
                {
                    Debug.Log("¡Tocaste un objeto interactuable!");
                    hit.transform.gameObject.SetActive(false); // ejemplo: desactiva el plano
                }
            }
        }
    }
}

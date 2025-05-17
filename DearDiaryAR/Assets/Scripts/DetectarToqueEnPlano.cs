using UnityEngine;

public class DetectarToqueEnPlano : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Toque detectado en: " + hit.transform.name);

                if (hit.transform == this.transform)
                {
                    Debug.Log("¡Tocaste el plano!");
                }
            }
        }
    }
}

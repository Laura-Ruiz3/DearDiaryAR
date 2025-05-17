using UnityEngine;

public class PlaneTouchToggle : MonoBehaviour
{
    public GameObject objetoADesactivar; // Asigna esto desde el Inspector

    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform == this.transform)
                    {
                        ToggleObjeto();
                    }
                }
            }
        }
    }

    void ToggleObjeto()
    {
        if (objetoADesactivar != null)
        {
            bool estadoActual = objetoADesactivar.activeSelf;
            objetoADesactivar.SetActive(!estadoActual);
        }
    }
}

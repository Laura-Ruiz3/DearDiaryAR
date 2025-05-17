using UnityEngine;

public class PlaneTouchRotate : MonoBehaviour
{
    private bool rotated = false;

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
                        RotatePlane();
                    }
                }
            }
        }
    }

    void RotatePlane()
    {
        // Alterna entre 0 y 180 grados en el eje Y
        float targetYRotation = rotated ? 0f : 180f;
        transform.localRotation = Quaternion.Euler(0, targetYRotation, 0);
        rotated = !rotated;
    }
}

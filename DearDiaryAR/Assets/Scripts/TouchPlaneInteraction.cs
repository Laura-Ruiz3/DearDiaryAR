using UnityEngine;

public class TouchPlaneInteraction : MonoBehaviour
{
    [SerializeField] private LayerMask planeLayer;
    [SerializeField] private float touchMaxDistance = 10f;

    // Visual feedback for debugging (optional)
    [SerializeField] private GameObject touchIndicatorPrefab;

    private Camera arCamera;

    void Start()
    {
        // Get the main camera (AR camera)
        arCamera = Camera.main;

        if (arCamera == null)
        {
            Debug.LogError("No main camera found! Make sure your AR camera is tagged as MainCamera.");
        }
    }

    void Update()
    {
        // Check for touches on mobile
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.touches[0].position;
            HandleTouch(touchPosition);
        }

        // Also handle mouse clicks for testing in editor
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
#endif
    }

    private void HandleTouch(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        // Draw the ray in the Scene view for debugging
        Debug.DrawRay(ray.origin, ray.direction * touchMaxDistance, Color.red, 1.0f);

        if (Physics.Raycast(ray, out hit, touchMaxDistance, planeLayer))
        {
            // We've hit a plane!
            GameObject hitPlane = hit.collider.gameObject;

            // Get touch position on the plane
            Vector3 touchPosOnPlane = hit.point;

            // Get touch normal direction
            Vector3 touchNormal = hit.normal;

            // Show visual feedback (optional)
            if (touchIndicatorPrefab != null)
            {
                GameObject indicator = Instantiate(touchIndicatorPrefab, touchPosOnPlane,
                    Quaternion.FromToRotation(Vector3.up, touchNormal));

                // Optionally destroy indicator after some time
                Destroy(indicator, 2.0f);
            }

            // Execute action
            ExecutePlaneAction(hitPlane, touchPosOnPlane, touchNormal);
        }
    }

    private void ExecutePlaneAction(GameObject plane, Vector3 position, Vector3 normal)
    {
        Debug.Log("Plane touched: " + plane.name + " at position: " + position);

        // Example action: Change the plane's color
        MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            );
        }

        // Add your custom plane interaction code here
        // For example:
        // - Spawn objects at the touch position
        // - Modify the plane
        // - Trigger animations or effects
    }
}
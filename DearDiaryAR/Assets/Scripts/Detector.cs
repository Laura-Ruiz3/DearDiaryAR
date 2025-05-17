using UnityEngine;
using Vuforia;

public class Detector : MonoBehaviour
{
    //[Header("Targets")]
    //[Tooltip("Assign all Vuforia ObserverBehaviours (targets) here")]
    public ObserverBehaviour[] ImageTargets;

    //[Header("Text Manager")]
    //[Tooltip("Assign the TextBoxMangment component here")]
    public TextBoxMangment textManager;
  
    private int currentTarget = -1;

    //void Awake()
    //{
    //    if (ImageTargets == null || ImageTargets.Length == 0)
    //        Debug.LogError("ImageTargets array is empty in Move1!");
    //    if (textManager == null)
    //        Debug.LogError("TextBoxMangment not assigned in Move1!");
    //}

    void OnEnable()
    {
        foreach (var target in ImageTargets)
            if (target != null)
                target.OnTargetStatusChanged += OnTargetStatusChanged;
    }

    void OnDisable()
    {
        foreach (var target in ImageTargets)
            if (target != null)
                target.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        int index = System.Array.IndexOf(ImageTargets, behaviour);

        if (status.Status == Status.TRACKED && index != -1 && index != currentTarget)
        {
            currentTarget = index;
            if (textManager != null)
            {
                textManager.markerDetected = true;
                textManager.ShowText(currentTarget);
            }
        }
        else if (index == currentTarget && status.Status != Status.TRACKED)
        {
            currentTarget = -1;
            if (textManager != null)
            {
                textManager.markerDetected = false;

                // Si el botón Continue está activo, cambia a default
                if (textManager.IsContinueActive())
                {
                    textManager.ShowText(-1);
                }
                // Si no, deja la UI igual: NO interrumpas la animación/corrutina
            }
        }
    }




}

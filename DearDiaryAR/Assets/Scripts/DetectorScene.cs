using UnityEngine;
using Vuforia;

public class DetectorScene : MonoBehaviour
{
    public ObserverBehaviour[] ImageTargets; //Arreglo de marcadores
    public TextBoxMangmentScene textManager; //Script que maneja la visualización de diálogos de las escenas con minijuegos
    private int currentTarget = -1; //Índice del marcador detectado

    void Awake()
    {
        if (ImageTargets == null || ImageTargets.Length == 0)
            Debug.LogError("ImageTargets array is empty in Move1!");
        if (textManager == null)
            Debug.LogError("TextBoxMangment not assigned in Move1!");
    }

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

    /*
   * Se llama cuando el estado de un marcador de Vuforia cambia.
   *
   * Args:
   *   behaviour: El ObserverBehaviour correspondiente al marcador detectado.
   *   status: El estado del marcador (TRACKED, NOT_FOUND, ...).
   */
    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        int index = System.Array.IndexOf(ImageTargets, behaviour);

        //Acciones cuando el marcador es detectado
        if (status.Status == Status.TRACKED && index != -1 && index != currentTarget)
        {
            currentTarget = index;
            if (textManager != null)
            {
                //Ejecuta el caso correspondiente al marcador
                textManager.markerDetected = true;
                textManager.ShowText(currentTarget);
            }
        }
        //Acciones cuando el marcador se pierde
        else if (index == currentTarget && status.Status != Status.TRACKED)
        {
            currentTarget = -1; //Va al default del switch-case de los diálogos.
            if (textManager != null)
            {
                textManager.markerDetected = false;
                //Muestra el texto correspondiente al default del switch-case
                if (textManager.IsContinueActive())
                {
                    textManager.ShowText(-1);
                }
            }
        }
    }




}

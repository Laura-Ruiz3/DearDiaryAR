using UnityEngine;

public class SceneChanger : MonoBehaviour
{
    public void CambiarAEscena2()
    {
        Fade.Instance.ChangeScene("V3");
    }

    public void CambiarAEscenaFinal()
    {
        Fade.Instance.ChangeScene("V4");
    }
}
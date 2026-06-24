using UnityEngine;

public class InspectManager : MonoBehaviour
{
    private Vector3 glassStartPos = new(0.25f, 0.0761f, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Singleton.Instance.PutPlateAtOrigin();
        Singleton.Instance.PutGlass(glassStartPos, Quaternion.identity);
    }

    public void DoneInspecting()
    {
        Singleton.Instance.ChangeToPreviousScene();
    }
}

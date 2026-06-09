using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }
    public GameObject foodToPlace;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

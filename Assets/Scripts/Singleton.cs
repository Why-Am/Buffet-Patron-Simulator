using UnityEngine;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }
    public GameObject foodToPlace;

    [SerializeField]
    private GameObject platePrefab;
    [SerializeField]
    private GameObject plate;

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

    public GameObject InstantiatePlateAtOriginIfDoesNotExist()
    {
        if (plate == null)
        {
            plate = Instantiate(platePrefab, Vector3.zero, Quaternion.identity);
        }

        return plate;
    }
}

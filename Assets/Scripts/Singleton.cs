using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public GameObject foodToPlace;

    [SerializeField]
    private GameObject platePrefab;

    private GameObject plate;

    private string previousSceneName;

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

    public GameObject PutPlate(Vector3 position, Quaternion rotation)
    {
        if (plate == null)
        {
            Debug.Log("Instantiating new plate");
            plate = Instantiate(platePrefab, position, rotation);
            DontDestroyOnLoad(plate);
        }
        else
        {
            Debug.Log("Using existing plate");
            plate.transform.SetPositionAndRotation(position, rotation);
        }

        return plate;
    }

    public GameObject PutPlateAtOrigin()
    {
        return PutPlate(Vector3.zero, Quaternion.identity);
    }

    public void ChangeScene(string name)
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(name);
    }

    public void ChangeToPreviousScene()
    {
        if (previousSceneName == null)
        {
            Debug.Log("There is no previous scene to go to");
            return;
        }

        ChangeScene(previousSceneName);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton : MonoBehaviour
{
    public static Singleton Instance { get; private set; }

    public GameObject foodToPlace;

    public float mouseSensitivity = 25f;

    [SerializeField]
    private GameObject platePrefab;

    private GameObject plate;

    private string previousSceneName;

    private Vector3? lastPlayerPosition;
    private Quaternion? lastPlayerRotation;

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
            plate = InstantiateNewPlate(position, rotation);
        }
        else
        {
            // Debug.Log("Using existing plate");
            plate.transform.SetPositionAndRotation(position, rotation);
        }

        return plate;
    }

    public GameObject InstantiateNewPlate(Vector3 position, Quaternion rotation)
    {
        if (plate != null)
        {
            Destroy(plate);
        }
        // Debug.Log("Instantiating new plate");
        plate = Instantiate(platePrefab, position, rotation);
        DontDestroyOnLoad(plate);
        return plate;
    }

    public GameObject PutPlateAtOrigin()
    {
        return PutPlate(Vector3.zero, Quaternion.identity);
    }

    public GameObject InstantiateNewPlateAtOrigin()
    {
        return InstantiateNewPlate(Vector3.zero, Quaternion.identity);
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
            // Debug.Log("There is no previous scene to go to");
            return;
        }

        ChangeScene(previousSceneName);
    }

    public bool TryGetLastPlayerPositionAndRotation(out Vector3 position, out Quaternion rotation)
    {
        if (lastPlayerPosition == null || lastPlayerRotation == null)
        {
            // Debug.Log("Couldn't get last player position and rotation");
            position = Vector3.zero;
            rotation = Quaternion.identity;
            return false;
        }

        position = lastPlayerPosition.GetValueOrDefault();
        rotation = lastPlayerRotation.GetValueOrDefault();
        // Debug.Log($"Using last position ({position}) and rotation ({rotation})");
        return true;
    }

    public void SetLastPlayerPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        lastPlayerPosition = position;
        lastPlayerRotation = rotation;
    }

    public void SetPlateCollisions(bool enabled)
    {
        if (plate == null) return;

        plate.GetComponent<MeshCollider>().enabled = enabled;
        foreach (var meshCollider in plate.GetComponentsInChildren<MeshCollider>())
        {
            meshCollider.enabled = enabled;
        }
    }
}

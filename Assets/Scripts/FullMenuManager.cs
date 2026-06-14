using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FullMenuManager : MonoBehaviour
{
    [SerializeField]
    private Transform scrollViewContent;
    [SerializeField]
    private GameObject buttonPrefab;

    void Start()
    {
        Singleton.Instance.PutPlateAtOrigin();
        PopulateScrollView();
    }

    void PopulateScrollView()
    {
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        GameObject[] foodPrefabs = Resources.LoadAll<GameObject>("FoodItems");

        foreach (GameObject foodPrefab in foodPrefabs)
        {
            GameObject buttonObject = Instantiate(buttonPrefab, scrollViewContent);

            TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            Button buttonComponent = buttonObject.GetComponent<Button>();

            if (buttonText == null)
            {
                Debug.LogError("Button needs a text component");
                return;
            }
            if (buttonComponent == null)
            {
                Debug.LogError("Button needs a button component");
                return;
            }

            buttonText.text = foodPrefab.name;
            buttonComponent.onClick.AddListener(() => GoToPlaceFood(foodPrefab));
        }
    }

    void GoToPlaceFood(GameObject foodPrefab)
    {
        Singleton.Instance.foodToPlace = foodPrefab;
        Singleton.Instance.ChangeScene("FoodPlacementScene");
    }

    public void ResetPlate()
    {
        Singleton.Instance.InstantiateNewPlateAtOrigin();
    }
}

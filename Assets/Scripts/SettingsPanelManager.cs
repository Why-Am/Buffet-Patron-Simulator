using TMPro;
using UnityEngine;

public class SettingsPanelManager : MonoBehaviour
{
    public bool IsOpen { get; private set; } = false;
    [SerializeField]
    private TMP_InputField inputField;

    void Start()
    {
        IsOpen = false;
        gameObject.SetActive(false);
        inputField.text = Singleton.Instance.mouseSensitivity.ToString();
    }

    public void ToggleOpen()
    {
        IsOpen = !IsOpen;
        gameObject.SetActive(IsOpen);
        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        // Debug.Log($"Settings panel IsOpen: {IsOpen}");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnMouseSensitivityInputFieldValueChanged()
    {
        if (!float.TryParse(inputField.text, out float result))
        {
            // Debug.LogError($"Couldn't parse {inputField.text} as float");
            return;
        }
        Singleton.Instance.mouseSensitivity = result;
        // Debug.Log($"Updated mouse sensitivity to {result}");
    }
}

using TMPro;
using UnityEngine;

public class InputModeUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI inputModeText;

    public void SetInputModeText (string text)
    {
        inputModeText.text = text;
    }
}
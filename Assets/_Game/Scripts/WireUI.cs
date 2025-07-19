using TMPro;
using UnityEngine;

public class WireUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentWireRemainingText;
    [SerializeField] TextMeshProUGUI startWireRemainingText;
    [SerializeField] TextMeshProUGUI minusWireText;

    public void SetCurrentWireRemainingText (string current, string start)
    {
        currentWireRemainingText.text = current;
        startWireRemainingText.text = start;
    }

    public void SetMinusWireText (bool active, string value = "")
    {
        minusWireText.gameObject.SetActive(active);
        minusWireText.text = value;
    }
}
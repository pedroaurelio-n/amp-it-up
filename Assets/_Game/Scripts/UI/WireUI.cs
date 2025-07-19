using TMPro;
using UnityEngine;

public class WireUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentWireRemainingText;
    [SerializeField] TextMeshProUGUI startWireRemainingText;
    [SerializeField] TextMeshProUGUI minusWireText;
    [SerializeField] Color normalColor;
    [SerializeField] Color discountedColor;

    public void SetCurrentWireRemainingText (string current, string start)
    {
        currentWireRemainingText.text = current;
        startWireRemainingText.text = start;
    }

    public void SetMinusWireText (bool active, bool discounted = false, string value = "")
    {
        minusWireText.gameObject.SetActive(active);
        minusWireText.text = value;
        minusWireText.color = discounted ? discountedColor : normalColor;
    }
}
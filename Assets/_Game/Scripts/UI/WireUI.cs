using TMPro;
using UnityEngine;

public class WireUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentWireRemainingText;
    [SerializeField] TextMeshProUGUI startWireRemainingText;
    [SerializeField] GameObject minusWireContainer;
    [SerializeField] TextMeshProUGUI minusWireText;
    [SerializeField] GameObject percentageSymbol;
    [SerializeField] Color normalColor;
    [SerializeField] Color discountedColor;

    public void SetCurrentWireRemainingText (string current, string start)
    {
        currentWireRemainingText.text = current;
        startWireRemainingText.text = start;
    }

    public void SetMinusWireText (bool active, bool discounted = false, string value = "")
    {
        minusWireContainer.gameObject.SetActive(active);
        percentageSymbol.SetActive(discounted);
        minusWireText.text = value;
        minusWireText.color = discounted ? discountedColor : normalColor;
    }
}
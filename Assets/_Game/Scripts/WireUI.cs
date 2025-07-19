using TMPro;
using UnityEngine;

public class WireUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentWireRemainingText;
    [SerializeField] TextMeshProUGUI startWireRemainingText;

    public void SetCurrentWireRemainingText (string current, string start)
    {
        currentWireRemainingText.text = current;
        startWireRemainingText.text = start;
    }
}
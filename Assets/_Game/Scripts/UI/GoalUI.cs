using TMPro;
using UnityEngine;

public class GoalUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentStructuresText;
    [SerializeField] TextMeshProUGUI totalStructuresText;

    public void SetStructuresText (string current, string total)
    {
        currentStructuresText.text = current;
        totalStructuresText.text = total;
    }
}
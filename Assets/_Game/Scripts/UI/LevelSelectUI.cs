using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [SerializeField] GameObject[] levelButtons;

    void OnEnable ()
    {
        PlayerPrefs.SetInt($"Level_1", 1);
        PlayerPrefs.Save();
        
        foreach (GameObject button in levelButtons)
            button.SetActive(false);
        
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            if (PlayerPrefs.HasKey($"Level_{levelNumber}") && PlayerPrefs.GetInt($"Level_{levelNumber}", 0) == 1)
            {
                levelButtons[i].SetActive(true);
            }
        }
    }
}
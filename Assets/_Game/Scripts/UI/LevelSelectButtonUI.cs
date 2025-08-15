using UnityEngine;

public class LevelSelectButtonUI : MonoBehaviour
{
    [SerializeField] int levelIndex;

    public void Click ()
    {
        GameManager.GoToLevel(levelIndex);
    }
}
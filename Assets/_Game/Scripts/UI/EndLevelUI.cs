using UnityEngine;
using UnityEngine.UI;

public class EndLevelUI : MonoBehaviour
{
    [SerializeField] GameObject panelObject;
    [SerializeField] Button nextButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;
    
    bool _initialized;
    
    void Update ()
    {
        if (_initialized)
            return;
        LevelManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        _initialized = true;
    }

    void HandleLevelCompleted ()
    {
        panelObject.SetActive(true);
        nextButton.onClick.AddListener(GameManager.AdvanceLevel);
        restartButton.onClick.AddListener(GameManager.RestartLevel);
        menuButton.onClick.AddListener(GameManager.GoToMainMenu);
    }
}

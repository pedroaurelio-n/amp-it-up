using UnityEngine;
using UnityEngine.UI;

public class EndLevelUI : MonoBehaviour
{
    [SerializeField] GameObject panelObject;
    [SerializeField] Button nextButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button menuButton;
    [SerializeField] bool forceInitialize;
    
    bool _initialized;
    
    void Update ()
    {
        if (_initialized && !forceInitialize)
            return;
        if (forceInitialize && !_initialized)
            HandleLevelCompleted();
        LevelManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        _initialized = true;
    }

    void HandleLevelCompleted ()
    {
        panelObject?.SetActive(true);
        nextButton?.onClick.AddListener(GameManager.AdvanceLevel);
        restartButton?.onClick.AddListener(GameManager.RestartLevel);
        menuButton?.onClick.AddListener(GameManager.GoToMainMenu);
    }
}

using UnityEngine.SceneManagement;

public static class GameManager
{
    public static void GoToMainMenu ()
    {
        SceneManager.LoadScene(0);
    }

    public static void AdvanceLevel ()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        int chosenScene = nextIndex < SceneManager.sceneCountInBuildSettings ? nextIndex : 0;
        SceneManager.LoadScene(chosenScene);
    }

    public static void RestartLevel ()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

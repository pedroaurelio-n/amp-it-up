using UnityEngine;

public class Structure : MonoBehaviour
{
    bool isPowered;

    void Start ()
    {
        LevelManager.Instance.RegisterStructure(this);
    }

    public void SetPowered(bool powered)
    {
        if (isPowered == powered) return;

        isPowered = powered;
        if (isPowered)
            OnPowered();
        else
            OnDepowered();
    }

    void OnPowered()
    {
        Debug.Log($"{name} is powered!");
    }

    void OnDepowered()
    {
        Debug.Log($"{name} lost power.");
    }
}
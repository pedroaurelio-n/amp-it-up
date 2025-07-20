using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Pole : MonoBehaviour
{
    public bool IsPowered { get; private set; }

    Coroutine animationCoroutine;

    void Start ()
    {
        LevelManager.Instance.RegisterPole(this);
    }

    public void SetPowered(bool powered)
    {
        if (IsPowered == powered) return;

        IsPowered = powered;
        // if (IsPowered)
        //     OnPowered();
        // else
        //     OnDepowered();
    }

    public void Click ()
    {
        if (animationCoroutine != null)
            return;
        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine ()
    {
        bool complete = false;
        LevelManager.Instance.TriggerOnClickEntity();
        transform.GetChild(0).GetChild(0).DOPunchScale(Vector3.one * -0.15f, 1f, 3).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        animationCoroutine = null;
    }
}
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Structure : MonoBehaviour
{
    bool isPowered;

    Coroutine animationCoroutine;
    Coroutine clickAnimationCoroutine;
    
    StructureParticles particles;

    void Start ()
    {
        LevelManager.Instance.RegisterStructure(this);
        particles = GetComponentInChildren<StructureParticles>();
    }

    public void SetPowered(bool powered)
    {
        bool oldState = isPowered;
        if (isPowered == powered) return;

        isPowered = powered;
        if (isPowered)
            OnPowered();
        else
            OnDepowered();
    }

    void OnPowered()
    {
        if (animationCoroutine != null)
            return;
        animationCoroutine = StartCoroutine(AnimationRoutine());
    }

    void OnDepowered()
    {
        if (animationCoroutine == null)
            return;
        DOTween.KillAll();
        StopCoroutine(animationCoroutine);
        animationCoroutine = null;
        transform.GetChild(0).GetChild(0).DOScale(Vector3.one, 0.3f);
        particles.Constant.Stop();
    }

    IEnumerator AnimationRoutine ()
    {
        bool canShake = false;
        LevelManager.Instance.TriggerStructurePowered();
        yield return new WaitForSeconds(0.3f);
        
        transform.GetChild(0).GetChild(0).DOPunchScale(Vector3.one * 0.25f, 1f, 3).OnComplete(() => canShake = true);
        particles.Burst.Play();
        particles.Constant.Play();
        yield return new WaitUntil(() => canShake);
        while (canShake)
        {
            bool canChange = false;
            transform.GetChild(0).GetChild(0).DOScale(Vector3.one * 1.1f,0.3f).SetEase(Ease.Linear).OnComplete(() => canChange = true);
            yield return new WaitUntil(() => canChange);

            canChange = false;
            transform.GetChild(0).GetChild(0).DOScale(Vector3.one * 0.9f,0.3f).SetEase(Ease.Linear).OnComplete(() => canChange = true);
            yield return new WaitUntil(() => canChange);
            yield return null;
        }
    }
    
    public void Click ()
    {
        if (clickAnimationCoroutine != null || animationCoroutine != null)
            return;
        clickAnimationCoroutine = StartCoroutine(ClickAnimationRoutine());
    }

    IEnumerator ClickAnimationRoutine ()
    {
        bool complete = false;
        LevelManager.Instance.TriggerOnClickEntity();
        transform.GetChild(0).GetChild(0).DOPunchScale(Vector3.one * -0.15f, 1f, 3).OnComplete(() => complete = true);
        yield return new WaitUntil(() => complete);
        clickAnimationCoroutine = null;
    }
}
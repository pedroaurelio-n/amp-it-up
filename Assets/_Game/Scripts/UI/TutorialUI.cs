using DG.Tweening;
using PedroAurelio.AudioSystem;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] PlayAudioEvent audioEvent;
    [SerializeField] float tweenDuration;
    [SerializeField] TextMeshPro text;

    RectTransform _rect;
    Vector3 _startPosition;
    
    void Start ()
    {
        _rect = GetComponent<RectTransform>();
        text.DOFade(0f, 0.01f);
        _startPosition = _rect.position;
        _rect.position = new Vector3(_startPosition.x, _startPosition.y, _startPosition.z - 2f);

        text.DOFade(1f, tweenDuration * 3f);
        _rect.DOMoveZ(_startPosition.z, tweenDuration).OnComplete(() =>
        {
            _rect.DOPunchScale(Vector3.one * 0.10f, 0.5f, 3);
            audioEvent.PlayAudio();
        });
    }
}

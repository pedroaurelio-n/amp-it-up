using PedroAurelio.AudioSystem;
using UnityEngine;

public class AudioEventListener : MonoBehaviour
{
    [SerializeField] PlayAudioEvent wirePowerAudio;
    [SerializeField] PlayAudioEvent structurePowerAudio;
    [SerializeField] PlayAudioEvent wireDestroyedAudio;
    [SerializeField] PlayAudioEvent wirePlacedAudio;
    [SerializeField] PlayAudioEvent ghostWireUpdatedAudio;
    [SerializeField] PlayAudioEvent wireImpossibleAudio;
    [SerializeField] PlayAudioEvent victoryAudio;
    [SerializeField] PlayAudioEvent defeatAudio;
    [SerializeField] PlayAudioEvent clickAudio;
    
    bool _initialized;

    void Update ()
    {
        if (_initialized)
            return;

        LevelManager.Instance.OnWirePowered += HandleWirePowered;
        LevelManager.Instance.OnStructurePowered += HandleStructuredPowered;
        LevelManager.Instance.OnWireDestroyed += HandleWireDestroyed;
        LevelManager.Instance.OnWirePlaced += HandleWirePlaced;
        LevelManager.Instance.OnGhostWireUpdated += HandleOnGhostWireUpdated;
        LevelManager.Instance.OnWireImpossible += HandleOnWireImpossible;
        LevelManager.Instance.OnDefeat += HandleDefeat;
        LevelManager.Instance.OnVictory += HandleVictory;
        LevelManager.Instance.OnClickEntity += HandleClick;
        _initialized = true;
    }
    
    void HandleClick ()
    {
        clickAudio.PlayAudio();
    }
    
    void HandleVictory ()
    {
        victoryAudio.PlayAudio();
    }
    
    void HandleDefeat ()
    {
        defeatAudio.PlayAudio();
    }
    
    void HandleOnWireImpossible ()
    {
        wireImpossibleAudio.PlayAudio();
    }
    
    void HandleOnGhostWireUpdated ()
    {
        ghostWireUpdatedAudio.PlayAudio();
    }

    void HandleWirePowered ()
    {
        wirePowerAudio.PlayAudio();
    }

    void HandleStructuredPowered ()
    {
        Invoke(nameof(PlayStructureAudio), 0.2f);
    }

    void HandleWirePlaced ()
    {
        wirePlacedAudio.PlayAudio();
    }
    
    void HandleWireDestroyed ()
    {
        wireDestroyedAudio.PlayAudio();
    }

    void PlayStructureAudio ()
    {
        structurePowerAudio.PlayAudio();
    }
}
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public VoiceManager voiceManager;
    private bool isHidding = false;

    public void Hide()
    {
        isHidding = true;
    }

    public void Unhide ()
    {
        isHidding = false;
    }

    public void Distraction(Vector3 voicePosition)
    {
        voiceManager.MakeVoice(voicePosition);
    }

    public bool isPlayerHidding()
    {
        return isHidding;
    }
}

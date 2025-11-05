using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    public EnemyBase[] listenerList;

    public void MakeVoice (Vector3 voicePosition)
    {
        for (int i = 0; i < listenerList.Length; i++)
        {
            if (listenerList[i].hearingDistance >= distanceTo(voicePosition, listenerList[i].transform.position))
            {
                listenerList[i].HearSound(voicePosition);
            }
        }
    }

    private float distanceTo (Vector3 position1,  Vector3 position2)
    {
        Vector3 dirToPlayer = position1 - position2;
        float distance = dirToPlayer.magnitude;
        Debug.Log(distance);
        return distance;
    }
}

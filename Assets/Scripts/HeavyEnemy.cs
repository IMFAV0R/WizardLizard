using UnityEngine;

public class HeavyEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        visionDistance = 15f;
        visionAngle = 70f;
        hearingDistance = 6f;
    }

    public override void HearSound(Vector3 soundPos)
    {
        base.HearSound(soundPos);
    }
}

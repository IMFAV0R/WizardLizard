using UnityEngine;

public class BasicEnemy : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        visionDistance = 10f;
        visionAngle = 40f;
        hearingDistance = 0f;
    }
}

using UnityEngine;
using System.Collections;


public abstract class EnemyAttackPattern : ScriptableObject
{
    public float attackRange;
    public float attackChargeSec;
    public float attackDuration;
    public float attackPostDelay;
    public bool freezeDuringCast = false;
    public abstract IEnumerator Execute(EnemyBase enemy);

}


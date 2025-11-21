using System;
using Unity.Behavior;

[BlackboardEnum]
public enum EBossState
{
	SPAWN,
    IDLE,
	CHASE,
	ATTACK,
	DAMAGED,
	DEATH
}

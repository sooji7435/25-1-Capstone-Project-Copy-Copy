using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.Sentis;
using UnityEngine;

[RequireComponent(typeof(BehaviorParameters))]
public class Boss_infer : Agent
{
    public Transform player;
    private Rigidbody2D rb;
    private Fuzzy fuzzy;

    private Boss boss; 

    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float repulsionRange = 1.2f;
    public LayerMask wallMask;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boss = GetComponent<Boss>();
    }

    private void FixedUpdate()
    {
        if (boss == null || player == null) return;
        if (boss.IsDead()) return;

        float bossHP = boss.GetHealthRatio();
        float playerHP = boss.GetPlayerHealthRatio();
        float dist = Vector2.Distance(transform.position, player.position);

        // 퍼지 로직으로 현재 성향 계산
        Vector3 intent = fuzzy.Evaluate(dist, bossHP, playerHP);
        float aggr = intent.x;
        float defe = intent.y;
        float balance = intent.z;

        // 공격 / 수비 / 밸런스형에 따라 유지 거리 설정
        float desiredRange = aggr * 2.0f + balance * 3.0f + defe * 5.0f;

        // 거리 오차
        float distError = dist - desiredRange;
        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // 이동 방향 계산 (가까우면 뒤로, 멀면 앞으로)
        Vector2 moveDir = (distError > 0 ? dirToPlayer : -dirToPlayer).normalized;

        // 벽 리펄션
        Vector2 repulsion = Vector2.zero;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, repulsionRange, wallMask);
        if (hit.collider != null)
        {
            repulsion = -moveDir * (repulsionRange - hit.distance);
        }

        // 실제 이동
        rb.linearVelocity = (moveDir + repulsion).normalized * moveSpeed;

        // 공격 판단
        if (aggr > 0.7f && dist <= desiredRange + 0.3f)
        {
            boss.TryAttack(); // 공격 시도 (BossFSM 내부에서 처리)
        }
    }
}

using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// 🎯 BossAgent_Combat_Revised
/// - 퍼지(Fuzzy) + 강화학습(RL) 융합형 보스 전투 AI
/// - 입력: 거리(dist), 보스HP, 플레이어HP
/// - 출력: 공격적/수비적/균형형 패턴 학습 및 거리/이동 제어
/// - 보상: 퍼지 성향에 따라 동적으로 목표 거리(desiredRange) 변화
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BossAgent : Agent
{
    [Header("References")]
    public Transform player;
    public LayerMask wallMask;
    private Rigidbody2D rb;
    private Fuzzy fuzzy = new();

    [Header("Movement Settings")]
    public float moveForce = 12f;
    public float maxSpeed = 6f;
    public float targetRange = 3f;

    [Header("Combat Parameters (Dynamic)")]
    [Range(0f, 1f)] public float bossHP;
    [Range(0f, 1f)] public float playerHP;

    private Vector2 lastMove;
    private float episodeTime;

    // ------------------------------------------------------
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnEpisodeBegin()
    {
        // 초기화
        rb.linearVelocity = Vector2.zero;
        episodeTime = 0f;
        fuzzy.ResetParameters();

        // 👇 매 시나리오마다 랜덤 체력 부여
        bossHP = Random.Range(0.3f, 1.0f);
        playerHP = Random.Range(0.3f, 1.0f);

        // 👇 랜덤 위치 스폰 (환경 다양화)
        transform.position = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
        player.position = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));

        Debug.Log($"[EPISODE START] BossHP:{bossHP:F2}, PlayerHP:{playerHP:F2}");
    }

    // ------------------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        Vector2 toP = (Vector2)(player.position - transform.position);
        float dist = toP.magnitude;

        // ① 방향 + 거리
        sensor.AddObservation(toP.normalized);
        sensor.AddObservation(Mathf.Clamp(dist / 10f, 0f, 1f));

        // ② 속도
        sensor.AddObservation(rb.linearVelocity / maxSpeed);

        // ③ 체력 상태
        sensor.AddObservation(bossHP);
        sensor.AddObservation(playerHP);

        // ④ 퍼지 파라미터 (학습 대상)
        sensor.AddObservation(fuzzy.muNear);
        sensor.AddObservation(fuzzy.sigmaNear);
        sensor.AddObservation(fuzzy.muFar);
        sensor.AddObservation(fuzzy.sigmaFar);
        sensor.AddObservation(fuzzy.wAggressive);
        sensor.AddObservation(fuzzy.wDefensive);
    }

    // ------------------------------------------------------
    public override void OnActionReceived(ActionBuffers actions)
    {
        episodeTime += Time.fixedDeltaTime;
        var a = actions.ContinuousActions;

        // 퍼지 파라미터 학습
        fuzzy.muNear = Mathf.Clamp(fuzzy.muNear + a[0] * 0.10f, 0.2f, 5f);
        fuzzy.sigmaNear = Mathf.Clamp(fuzzy.sigmaNear + a[1] * 0.05f, 0.1f, 2.5f);
        fuzzy.muFar = Mathf.Clamp(fuzzy.muFar + a[2] * 0.10f, 3f, 10f);
        fuzzy.sigmaFar = Mathf.Clamp(fuzzy.sigmaFar + a[3] * 0.05f, 0.1f, 3.0f);
        fuzzy.wAggressive = Mathf.Clamp(fuzzy.wAggressive + a[4] * 0.05f, 0.1f, 2.0f);
        fuzzy.wDefensive = Mathf.Clamp(fuzzy.wDefensive + a[5] * 0.05f, 0.1f, 2.0f);

        // 이동 입력
        Vector2 moveInput = new(a[6], a[7]);
        lastMove = Vector2.ClampMagnitude(moveInput, 1f);

        // ------------------------------
        // 🎯 퍼지 평가
        float dist = Vector2.Distance(transform.position, player.position);
        Vector3 intent = fuzzy.Evaluate(dist, bossHP, playerHP);
        float aggr = intent.x;
        float defe = intent.y;
        float balance = intent.z;

        Vector2 dirToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;

        // 🎯 퍼지 기반 목표 거리
        float desiredRange =
            aggr * 2.0f +
            balance * 5.0f +
            defe * 9.0f;

        // 거리 오차
        float distError = dist - desiredRange;

        // ------------------------------
        // 🧱 벽 회피 로직
        Vector2 repulsion = Vector2.zero;
        float repulsionRadius = 1.5f;
        float repulsionStrength = 3f;

        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, repulsionRadius, wallMask);
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, repulsionRadius, wallMask);
        RaycastHit2D hitUp = Physics2D.Raycast(transform.position, Vector2.up, repulsionRadius, wallMask);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, repulsionRadius, wallMask);

        if (hitLeft.collider != null) repulsion += Vector2.right * (repulsionRadius - hitLeft.distance);
        if (hitRight.collider != null) repulsion += Vector2.left * (repulsionRadius - hitRight.distance);
        if (hitUp.collider != null) repulsion += Vector2.down * (repulsionRadius - hitUp.distance);
        if (hitDown.collider != null) repulsion += Vector2.up * (repulsionRadius - hitDown.distance);

        // ------------------------------
        // 🚶 이동 방향 계산
        Vector2 moveDir =
            (distError * dirToPlayer) // 거리 보정
            + (repulsion * repulsionStrength * Time.fixedDeltaTime); // 벽 반발 추가

        rb.linearVelocity = moveDir.normalized * maxSpeed;

        // ------------------------------
        // 🏆 보상 설계
        float reward = 0f;

        if (Mathf.Abs(distError) < 0.5f)
            reward += +0.03f;
        else if (dist < desiredRange)
            reward += distError * 0.1f;  // 너무 가까우면 감점
        else
            reward += -(distError) * 0.05f; // 너무 멀면 감점

        // 벽 감점
        if (repulsion.magnitude > 0f)
            reward -= 0.02f; // 벽 근처면 감점

        // 이동 안정성 + 생존 보상
        reward += -Vector2.Distance(moveInput, lastMove) * 0.005f;
        reward += +0.001f;

        AddReward(reward);

        // ------------------------------
        // 로그 출력
        if (StepCount % 30 == 0)
        {
            string behavior = fuzzy.GetDominantBehavior(dist, bossHP, playerHP);
            Debug.Log(
                $"[Step:{StepCount}] " +
                $"Dist:{dist:F2}, Desired:{desiredRange:F2}, Err:{distError:F2}, " +
                $"Behavior:{behavior}, Repulsion:{repulsion.magnitude:F2}, " +
                $"RewardΔ:{reward:F3}, Total:{GetCumulativeReward():F3}"
            );
        }

        // 종료
        if (dist > 15f || episodeTime > 30f)
        {
            Debug.Log($"[EPISODE END] RewardSum:{GetCumulativeReward():F2}\n");
            EndEpisode();
        }
    }

}


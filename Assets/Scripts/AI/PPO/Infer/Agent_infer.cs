using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


/// <summary>
/// ✅ 실제 게임용 추론 전용 몬스터 AI + 시각화 디버그
/// - 학습된 ONNX 모델로 행동 결정
/// - Scene 뷰에서 타깃 인식 방향, 라이다, 이동 방향 표시
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Agent_Infer : Agent
{
    [Header("References")]
    public Transform player;             // 플레이어 (자동 연결됨)
    public LayerMask obstacleMask;       // 장애물 감지용 라이다 마스크

    [Header("Movement")]
    public float moveSpeed = 2.5f;       // 이동 속도
    public float rotationSpeed = 10f;    // 회전 보정 속도 (시각적 회전용)

    [Header("Lidar Settings")]
    public int lidarRays = 24;
    public float lidarRange = 6f;

    private Rigidbody2D rb;
    private Vector2 lastAction;
    private Vector2 lastToPlayer; // 인식한 방향 (디버그용)

    // -----------------------------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 라이다 마스크 자동 설정
        if (obstacleMask == 0)
            obstacleMask = LayerMask.GetMask("Wall");

        // Player 자동 연결
        if (!player && PlayerScript.Instance != null)
            player = PlayerScript.Instance.transform;
    }

    void Update()
    {
        // 런타임 중 Player가 나중에 생성되면 자동 연결
        if (!player && PlayerScript.Instance != null)
        {
            player = PlayerScript.Instance.transform;
        }
    }

    // -----------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        // ✅ 라이다 입력 (24개)
        var d = LidarScan();
        foreach (var v in d) sensor.AddObservation(v);

        if (!player) return;

        // ✅ 플레이어 방향 및 거리
        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        lastToPlayer = toPlayer; // 디버그용 저장
        sensor.AddObservation(toPlayer.normalized);                   // 방향 (2)
        sensor.AddObservation(Mathf.Clamp01(toPlayer.magnitude / 8f)); // 거리 (1)

        // ✅ 속도 및 이전 행동
        sensor.AddObservation(rb.linearVelocity / Mathf.Max(1f, moveSpeed)); // (2)
        sensor.AddObservation(lastAction); // (2)
    }

    // -----------------------------------------------
    float[] LidarScan()
    {
        float[] distances = new float[lidarRays];
        float step = 360f / Mathf.Max(1, lidarRays);

        for (int i = 0; i < lidarRays; i++)
        {
            float ang = i * step * Mathf.Deg2Rad;
            Vector2 dir = new(Mathf.Cos(ang), Mathf.Sin(ang));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, lidarRange, obstacleMask);
            distances[i] = hit.collider ? hit.distance / lidarRange : 1f;

            // 라이다 디버그 표시 (빨간선)
            Debug.DrawRay(transform.position, dir * (hit.collider ? hit.distance : lidarRange), hit.collider ? Color.red : new Color(1f, 0.5f, 0f, 0.3f));
        }

        return distances;
    }

    // -----------------------------------------------
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector2 act = new(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        lastAction = Vector2.ClampMagnitude(act, 1f);

        rb.linearVelocity = transform.TransformDirection(lastAction) * moveSpeed;


        // 디버그 로그 (주기적으로 모델 출력 확인)
        if (StepCount % 200 == 0)
        {
            Debug.Log($"[Infer] Action={lastAction}, Speed={rb.linearVelocity.magnitude:F2}, DistToPlayer={lastToPlayer.magnitude:F2}");
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;
        a[0] = Input.GetAxisRaw("Horizontal");
        a[1] = Input.GetAxisRaw("Vertical");
    }

    // -----------------------------------------------
    // ✅ Scene 뷰 시각화
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!player) return;

        // 1️⃣ 에이전트 위치
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.15f);

        // 2️⃣ 타깃 위치
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(player.position, 0.15f);

        // 3️⃣ 타깃 인식 방향 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastToPlayer.normalized * 2f);

        // 4️⃣ 실제 이동 방향 (파란색)
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)lastAction * 2f);

#if UNITY_EDITOR
        // 5️⃣ 거리 표시
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.4f, $"Dist:{lastToPlayer.magnitude:F2}");
#endif
    }
}

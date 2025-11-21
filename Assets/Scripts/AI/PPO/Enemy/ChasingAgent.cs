using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody2D))]
public class ChasingAgent : Agent
{
    [Header("References")]
    public Transform target;
    public LayerMask wallMask;

    [Header("Movement")]
    public float speed = 3f;

    [Header("Lidar Settings")]
    public int lidarRays = 24;
    public float lidarRange = 6f;

    private Rigidbody2D rb;
    private Vector2 lastAction;
    private float prevDist = float.NaN;
    private float minWallDist = 999f;
    private Vector2 closestWallDir = Vector2.zero;


    // -------------------- Unity Lifecycle --------------------
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Debug.Log($"[WallMask Check] wallMask value: {wallMask.value}");
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector2.zero;

        // 안전한 타깃 위치 생성
        target.position = RandomSafePosition();

        // 에이전트 위치 생성 (타깃과 일정 거리 이상 떨어지게)
        transform.position = RandomSafePosition(target.position, minDistance: 2f, maxDistance: 9f);

        prevDist = float.NaN;
    }

    Vector2 RandomSafePosition(Vector2? avoidPos = null, float minDistance = 0f, float maxDistance = 999f)
    {
        Vector2 pos;
        int safety = 0;

        do
        {
            // 맵 내부 범위에서 랜덤 위치 생성
            pos = new Vector2(Random.Range(-11f, 11f), Random.Range(-6.5f, 6.5f));

            // 1. 장애물 피하기
            bool nearObstacle = Physics2D.OverlapCircle(pos, 0.5f);

            // 2. 회피 대상과의 거리 유지 (타깃과 너무 가깝지 않게)
            bool tooClose = false;
            if (avoidPos.HasValue)
            {
                float dist = Vector2.Distance(pos, avoidPos.Value);
                tooClose = dist < minDistance || dist > maxDistance;
            }

            if (!nearObstacle && !tooClose)
                break; // 안전한 위치 찾음

            safety++;

        } while (safety < 300);

        return pos;
    }


    // -------------------- Observations --------------------
    float[] LidarScan()
    {
        float[] d = new float[lidarRays];
        float step = 360f / Mathf.Max(1, lidarRays);
        minWallDist = lidarRange; // 최소 거리 초기화
        closestWallDir = Vector2.zero;

        for (int i = 0; i < lidarRays; i++)
        {
            float ang = i * step * Mathf.Deg2Rad;
            Vector2 dir = new(Mathf.Cos(ang), Mathf.Sin(ang));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, lidarRange, wallMask);

            if (hit.collider)
            {
                d[i] = hit.distance / lidarRange;

                // 가장 가까운 벽 기록
                if (hit.distance < minWallDist)
                {
                    minWallDist = hit.distance;
                    closestWallDir = dir;
                }

                // 디버그 라이다 (빨강)
                Debug.DrawRay(transform.position, dir * hit.distance, new Color(1f, 0f, 0f, 0.3f));
            }
            else
            {
                d[i] = 1f;
                Debug.DrawRay(transform.position, dir * lidarRange, new Color(1f, 0.5f, 0f, 0.1f));
            }
        }
        return d;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var d = LidarScan();
        foreach (var v in d) sensor.AddObservation(v);

        Vector2 toTarget = (Vector2)target.position - (Vector2)transform.position;
        sensor.AddObservation(toTarget.normalized);
        sensor.AddObservation(Mathf.Clamp01(toTarget.magnitude / 8f));
        sensor.AddObservation(rb.linearVelocity / Mathf.Max(1f, speed));
        sensor.AddObservation(lastAction);
    }

    // -------------------- Actions --------------------
    public override void OnActionReceived(ActionBuffers actions)
    {
        Vector2 act = new(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        lastAction = Vector2.ClampMagnitude(act, 1f);
        rb.linearVelocity = lastAction * speed;

        float currDist = Vector2.Distance(transform.position, target.position);

        // 거리 보상
        if (!float.IsNaN(prevDist))
        {
            float delta = prevDist - currDist;
            AddReward(delta * 2.0f);
        }
        prevDist = currDist;

        // 목표 도달
        if (currDist < 0.5f)
        {
            AddReward(+10f);
            EndEpisode();
        }

        // 장애물, 벽 충돌
        if (Physics2D.OverlapCircle(transform.position, 0.7f, wallMask))
        {
            AddReward(-2f);
            EndEpisode();
        }



    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;
        a[0] = Input.GetAxisRaw("Horizontal");
        a[1] = Input.GetAxisRaw("Vertical");
    }

}


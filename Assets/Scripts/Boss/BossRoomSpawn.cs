using System.Collections.Generic;
using UnityEngine;

public class BossRoomSpawn : MonoBehaviour
{
    [Header("스폰 포인트들 (자식 오브젝트들)")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 30f; // 몇 초마다 소환할지
    [SerializeField] private int spawnCount = 2; // 한 번에 2마리 스폰

    private float timer = 0f;
    private bool isActive = false;

    private void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            Spawn();
        }
    }

    /// <summary>
    /// 보스가 전투 시작할 때 호출해줄 함수
    /// </summary>
    public void StartSpawn()
    {
        isActive = true;
        timer = 0f;

        Debug.Log("스폰시작");
    }

    /// <summary>
    /// 보스가 죽었을 때 호출해줄 함수
    /// </summary>
    public void StopSpawn()
    {
        isActive = false;
    }

    private void Spawn()
    {
        Debug.Log("스폰");

        // 스폰포인트 List 생성
        List<int> indexes = new List<int>();

        // 랜덤 인덱스 2개 뽑기 (중복 X)
        while (indexes.Count < spawnCount)
        {
            int rand = Random.Range(0, spawnPoints.Length); // 0은 자기 자신일 가능성 있음
            if (!indexes.Contains(rand))
                indexes.Add(rand);
        }

        // 선택된 2개 지점에서 몬스터 스폰
        foreach (int idx in indexes)
        {
            Vector2 pos = spawnPoints[idx].position;
            EnemyManager.Instance.EnemySpawn(pos);
            Debug.Log($"[BossRoomSpawner] Spawn at point {idx}");
        }

    }
}

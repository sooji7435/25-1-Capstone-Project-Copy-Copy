using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 🎯 태그로 지정된 맵 Collider2D를 자동 인식해 카메라 이동을 맵 범위 안으로 제한.
/// - Cinemachine 전용 2D 확장
/// - 던전층 이동, 맵 교체 시 자동 갱신 가능
/// </summary>
[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Extensions/Cinemachine Auto Bounds By Tag 2D")]
public class CameraBounds : CinemachineExtension
{
    [Tooltip("맵 전체를 감싸는 오브젝트의 태그 (예: DungeonMap, BossMap 등)")]
    public string mapTag = "Map"; // 기본 태그 이름 (Project Settings > Tags에 등록 필요)

    private Collider2D mapCollider;
    private Vector2 center;
    private Vector2 mapSize;
    private float camHalfWidth, camHalfHeight;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body)
            return;

        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic)
            return;

        // 현재 맵 Collider 자동 탐색 (태그 기반)
        if (mapCollider == null)
        {
            GameObject taggedObj = GameObject.FindGameObjectWithTag(mapTag);
            if (taggedObj != null)
                mapCollider = taggedObj.GetComponent<Collider2D>();

            if (mapCollider == null)
                return;
        }

        // Collider 기반으로 center/mapSize 자동 계산
        Bounds b = mapCollider.bounds;
        center = b.center;
        mapSize = b.extents;

        // 카메라 시야 크기 계산
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;

        // 이동 제한 (Clamp)
        float lx = mapSize.x - camHalfWidth;
        float ly = mapSize.y - camHalfHeight;

        float clampX = Mathf.Clamp(state.RawPosition.x, -lx + center.x, lx + center.x);
        float clampY = Mathf.Clamp(state.RawPosition.y, -ly + center.y, ly + center.y);

        state.RawPosition = new Vector3(clampX, clampY, state.RawPosition.z);
    }

    /// <summary>
    /// 맵이 전환되었을 때 호출해서 Collider를 다시 탐색하게 함.
    /// </summary>
    public void ForceReacquireMap()
    {
        mapCollider = null;
        Debug.Log("[CinemachineAutoBoundsByTag2D] 맵 Collider를 태그로 다시 탐색 예정");
    }
}

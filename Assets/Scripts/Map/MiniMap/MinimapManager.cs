using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MinimapManager : Singleton<MinimapManager>
{

    [SerializeField] Transform minimapContainer; // minimap용 UI 부모 (예: Canvas 아래)
    [SerializeField] GameObject minimapIconPrefab; // minimap에 표시할 작은 아이콘
    [SerializeField] float roomGap;

    [Header("Colors")]
    [SerializeField] Color normalColor = new Color(0.6f, 0.6f, 0.6f, 1f); // 일반방
    [SerializeField] Color highlightColor = Color.white;                   // 현재방
    [SerializeField] Color previewColor = new Color(0.6f, 0.6f, 0.6f, 0.6f); // 알파 0.4로 투명


    private Dictionary<Vector2Int, GameObject> minimapIcons = new();
    private Dictionary<Vector2Int, Color> roomColors = new();
    private Vector2Int? previousRoom = null;

    public void InitMiniMap()
    {
        foreach (var icon in minimapIcons.Values)
            Destroy(icon);
        minimapIcons.Clear();
        roomColors.Clear();
        previousRoom = null;
    }
    public void RegisterRoom(Vector2Int roomPos)
    {
        GameObject icon = Instantiate(minimapIconPrefab, minimapContainer);
        icon.transform.localPosition = new Vector3(roomPos.x * roomGap, roomPos.y * roomGap, 0); // 20f = minimap 격자 간격
        icon.SetActive(false);
        minimapIcons.Add(roomPos, icon);
        SetRoomColor(roomPos, normalColor);
    }

    public void RevealRoom(Vector2Int roomPos)
    {
        if (minimapIcons.TryGetValue(roomPos, out GameObject icon))
        {
            icon.SetActive(true);
        }
    }

    public void HighlightRoom(Vector2Int roomPos)
    {
        if (previousRoom.HasValue &&
            minimapIcons.TryGetValue(previousRoom.Value, out GameObject prevIcon))
        {
            // 원래 색 복원
            if (roomColors.TryGetValue(previousRoom.Value, out Color originalColor))
                prevIcon.GetComponent<UnityEngine.UI.Image>().color = originalColor;
        }

        // 현재 방 강조
        if (minimapIcons.TryGetValue(roomPos, out GameObject currentIcon))
        {
            currentIcon.GetComponent<UnityEngine.UI.Image>().color = highlightColor;
            previousRoom = roomPos;
        }

        ShowAdjacentUnvisitedRooms(roomPos);
    }

    private void ShowAdjacentUnvisitedRooms(Vector2Int currentRoom)
    {
        // 상하좌우 방향 정의
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (var dir in directions)
        {
            Vector2Int neighbor = currentRoom + dir;
            if (minimapIcons.ContainsKey(neighbor))
            {
                GameObject icon = minimapIcons[neighbor];
                var img = icon.GetComponent<Image>();

                // 이미 방문해서 활성화된 방이면 스킵
                if (icon.activeSelf) continue;

                // 미탐색 방은 회색 반투명 표시
                icon.SetActive(true);
                img.color = previewColor;
            }
        }
    }

    public void SetRoomColor(Vector2Int roomPos, Color color)
    {
        if (minimapIcons.TryGetValue(roomPos, out GameObject icon))
        {
            icon.GetComponent<UnityEngine.UI.Image>().color = color;
            roomColors[roomPos] = color;
        }
    }

    public void SetMinimapVisible(bool visible)
    {
        if (minimapContainer != null)
            minimapContainer.gameObject.SetActive(visible);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🟦 보스방 씬 이름일 경우 미니맵 끄기
        if (scene.name == "BlueDragonBoss")
        {
            minimapContainer.gameObject.SetActive(false);
        }
        else
        {
            minimapContainer.gameObject.SetActive(true);
        }
    }


}
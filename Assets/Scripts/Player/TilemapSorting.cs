using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer))]
public class TilemapSorting : MonoBehaviour
{
    private TilemapRenderer tr;

    void Awake()
    {
        tr = GetComponent<TilemapRenderer>();
    }

    void LateUpdate()
    {
        tr.sortingOrder = Mathf.RoundToInt(-transform.position.y * 100);
    }
}

using UnityEngine;
using System.Collections;

public enum Direction { Up, Down, Left, Right }
public class DoorTrigger : MonoBehaviour
{
    public Direction direction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (MapManager.isTeleportLocked)
            {
                return;
            }

            MapManager.Instance.StartTeleport(direction);

            FirebaseUploader uploader = Object.FindFirstObjectByType<FirebaseUploader>();


        }
    }

}

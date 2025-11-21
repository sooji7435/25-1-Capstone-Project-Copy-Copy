using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float cameraZ = -10f; // 카메라의 Z축 위치
    [SerializeField] CinemachineCamera cineCam;
    private CinemachineConfiner2D confiner;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        // mainCamera.transform.position = new Vector3(0, 0, cameraZ); // 초기 카메라 위치 설정
        confiner = cineCam.GetComponent<CinemachineConfiner2D>();
    }

    public void SetCameraPosition(Vector3 position)
    {
        // mainCamera.transform.position = new Vector3(position.x, position.y, cameraZ);
        cineCam.Target.TrackingTarget = PlayerScript.Instance.transform;
    }

    public void SetCameraBound()
    {
        StartCoroutine(ApplyBoundNextFrame());
    }

    private IEnumerator ApplyBoundNextFrame()
    {
        yield return null; // collider 활성화 대기

        var room = MapManager.Instance.GetCurrentRoom();
        if (room == null) yield break;

        Transform boundaryObj = room.transform.Find("RoomBoundary");

        Debug.Log("찾음");

        if (boundaryObj == null) yield break;

        var col = boundaryObj.GetComponent<Collider2D>();
        if (col == null) yield break;

        //// collider가 활성화될 때까지 또 한 프레임 대기
        //yield return null;

        confiner.BoundingShape2D = col;
        
        confiner.InvalidateBoundingShapeCache();

    }
    public IEnumerator DelayedSetCameraBound()
    {
        yield return null; // 한 프레임 대기
        SetCameraBound();  // collider 활성화 이후 적용됨
    }

    public IEnumerator LerpCameraPosition(Vector3 position)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = mainCamera.transform.position;
        while (elapsedTime < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, new Vector3(position.x, position.y, cameraZ), elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    public void CameraShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }
    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPosition = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalPosition;
    }

    /*
    public void SetActiveCineCam(bool active)
    {
        if (active == true)
        {
            cineCam.Follow = PlayerScript.Instance.GetPlayerTransform();
            cineCam.gameObject.SetActive(true);
        }
        else { cineCam.gameObject.SetActive(false); }

    }
    /*
    public void EnableFollowPlayer(bool active)
    {
        if (active)
        {
            var player = PlayerScript.Instance.GetPlayerTransform();
            cineCam.Follow = player;
            cineCam.LookAt = player;
            cineCam.gameObject.SetActive(true);
        }
        else
        {
            cineCam.gameObject.SetActive(false);
        }
    }
    */
}

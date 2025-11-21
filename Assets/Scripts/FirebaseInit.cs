using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseAuth auth;
    public Action OnFirebaseInitialized;

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("[Init] Firebase 초기화 완료");
                OnFirebaseInitialized?.Invoke(); // 메인 스레드에서 호출
            }
            else
            {
                Debug.LogError("[Init] Firebase 초기화 실패: " + task.Result);
            }
        });
    }
}

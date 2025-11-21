using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using System.Collections;

public class FirebaseAuthLogin : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public Button loginButton;

    void Start()
    {
        loginButton.interactable = false; // 초기엔 잠궈두기
        StartCoroutine(EnsureFirebaseReadyAndWireUp());
    }

    IEnumerator EnsureFirebaseReadyAndWireUp()
    {
        // 1) 이미 초기화되어 있으면 바로 버튼 활성화
        if (FirebaseInit.auth != null)
        {
            loginButton.interactable = true;
        }
        else
        {
            // 2) 씬에 FirebaseInit 있는지 찾아보고, 있으면 이벤트 구독
            var init = FindFirstObjectByType<FirebaseInit>();
            if (init != null)
            {
                init.OnFirebaseInitialized += () => { loginButton.interactable = true; };
            }
            else
            {
                // 3) 정말 없으면 이 씬에서 최소 초기화 (비권장이나 안전장치)
                bool ready = false;
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    if (task.Result == DependencyStatus.Available)
                    {
                        FirebaseInit.auth = FirebaseAuth.DefaultInstance;
                        ready = true;
                        Debug.Log("[Login] 로그인 씬에서 직접 Firebase 초기화 완료");
                    }
                    else
                    {
                        Debug.LogError("[Login] Firebase 의존성 실패: " + task.Result);
                    }
                });
                while (!ready) yield return null;
                loginButton.interactable = true;
            }
        }

        // 버튼 핸들러 연결(중복 연결 방지)
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(OnLoginClicked);
        Debug.Log("[Login] 버튼 활성화 & 핸들러 연결 완료");
    }

    void OnLoginClicked()
    {
        if (FirebaseInit.auth == null) { Debug.LogError("[Login] Firebase 미초기화"); return; }

        string email = emailInput.text.Trim();
        string pw = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw))
        { Debug.LogWarning("[Login] 입력값 누락"); return; }

        loginButton.interactable = false; // 중복 클릭 방지

        FirebaseInit.auth.SignInWithEmailAndPasswordAsync(email, pw)
        .ContinueWithOnMainThread(task =>
        {
            loginButton.interactable = true;

            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("[Login] 실패: " + task.Exception);
                return;
            }

            var user = task.Result.User;
            Debug.Log($"[Login] 성공: {user.Email} → Bootstrap으로 이동");
            SceneManager.LoadScene("Bootstrap");
        });
    }
}

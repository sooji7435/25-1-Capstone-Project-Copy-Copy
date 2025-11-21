// Assets/Scripts/FirebaseAuthSignup.cs
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Extensions;
using System.Collections;

public class FirebaseAuthSignup : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public Button signupButton;

    void Start()
    {
        signupButton.interactable = false;   // 초기엔 잠궈두기
        StartCoroutine(EnsureFirebaseReadyAndWireUp());
    }

    IEnumerator EnsureFirebaseReadyAndWireUp()
    {
        // 1) 이미 초기화되어 있으면 바로 버튼 활성화
        if (FirebaseInit.auth != null) {
            signupButton.interactable = true;
        } else {
            // 2) 씬에 FirebaseInit 있는지 찾아보고, 있으면 이벤트 구독
            var init = FindFirstObjectByType<FirebaseInit>();
            if (init != null) {
                init.OnFirebaseInitialized += () => { signupButton.interactable = true; };
            } else {
                // 3) 정말 없으면 이 씬에서 최소 초기화 (비권장이나 안전장치)
                bool ready = false;
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                    if (task.Result == DependencyStatus.Available) {
                        FirebaseInit.auth = FirebaseAuth.DefaultInstance;
                        ready = true;
                    } else {
                        Debug.LogError("[Signup] Firebase 의존성 실패: " + task.Result);
                    }
                });
                // 초기화 완료까지 한 프레임씩 대기
                while (!ready) yield return null;
                signupButton.interactable = true;
            }
        }

        // 버튼 핸들러 연결 (중복 연결 방지 위해 여기서 1회만)
        signupButton.onClick.RemoveAllListeners();
        signupButton.onClick.AddListener(OnSignupClicked);
    }

    void OnSignupClicked()
    {
        if (FirebaseInit.auth == null) { Debug.LogError("[Signup] Firebase 미초기화"); return; }

        string email = emailInput.text.Trim();
        string pw = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw)) { Debug.LogWarning("[Signup] 입력값 누락"); return; }
        if (pw.Length < 6) { Debug.LogWarning("[Signup] 비밀번호 6자 이상 필요"); return; }

        signupButton.interactable = false; // 중복 클릭 방지
        FirebaseInit.auth.CreateUserWithEmailAndPasswordAsync(email, pw)
        .ContinueWithOnMainThread(task =>
        {
            signupButton.interactable = true;

            if (task.IsCanceled || task.IsFaulted) {
                Debug.LogError("[Signup] 실패: " + task.Exception);
                return;
            }

            Debug.Log("[Signup] 성공 → Login으로 이동");
            SceneManager.LoadScene("Login");
        });
    }
}

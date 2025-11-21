using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeUIManager : MonoBehaviour
{
    public Button SignupBtn;
    public Button LoginBtn;

    void Start()
    {
        SignupBtn.onClick.AddListener(() => {
            Debug.Log("[Welcome] 회원가입 클릭");
            SceneManager.LoadScene("Signup");
        });

        LoginBtn.onClick.AddListener(() => {
            Debug.Log("[Welcome] 로그인 클릭");
            SceneManager.LoadScene("Login");
        });
    }
}

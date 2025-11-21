using UnityEngine;


public class UIManager : Singleton<UIManager>
{
    [Header("UI 오브젝트")]

    [SerializeField] GameObject InGameUI;
    [SerializeField] GameObject MainMenuUI;
    [SerializeField] GameObject MainMenuOptionUI;
    [SerializeField] GameObject InGameOptionUI;




    [Header("인게임")]
    public PlayerStatUI playerStatUI;
    public ParryStackUI parryStackUI;
    public SkillUI skillUI;
    public SkillSelect skillSelect;
    
    public AbilityRewardSystem abilityUI;
    public UI_BossUI bossUI;
    public UI_DeadInfo deadInfo;
    public PauseUI pauseUI;

    protected override void Awake()
    {
        base.Awake();
        InGameUI.SetActive(false);
        MainMenuUI.SetActive(true);
    }
    public void SetActiveInGameUI(bool active)
    {
        InGameUI.SetActive(active);
    }
    public void SetActiveMainMenuUI(bool active)
    {
        MainMenuUI.SetActive(active);
    }
    public void ToggleMainMenuOptionUI()
    {
        MainMenuOptionUI.SetActive(!MainMenuOptionUI.activeSelf);
    }

    public void ToggleInGameOptionUI()
    {
        InGameOptionUI.SetActive(!InGameOptionUI.activeSelf);
    }



}

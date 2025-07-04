using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject PauseMenu;
    public GameObject Buttons;
    public GameObject BestiatyPanel;
    public GameObject SettingsPanel;
    public GameObject DevelopersPanel;
    public PlayerGUI playerGUI;
    public GameObject DamageIndicator;

    private GameObject[] Panels;
    Animator[] buttonAnimators;
    bool isPaused;

    void Start()
    {
        Panels = new GameObject[3] { BestiatyPanel, SettingsPanel, DevelopersPanel };
        buttonAnimators = Buttons.GetComponentsInChildren<Animator>();
        for (int i = 0; i < buttonAnimators.Length; i++)
        {
            buttonAnimators[i].GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        isPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0;
                GlobalInspector.PlayerAlive = false;
                PauseMenu.SetActive(true);
                isPaused = true;
                playerGUI.enabled = false;
            }
            else 
            {
                CloseOtherTabs();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1;
                GlobalInspector.PlayerAlive = true;
                PauseMenu.SetActive(false);
                isPaused = false;
                playerGUI.enabled = true;
            }
        }
    }

    private void CloseOtherTabs()
    {
        for (int i = 0; i < Panels.Length; i++)
            Panels[i].SetActive(false);
        foreach (Animator anim in buttonAnimators)
        {
            anim.SetBool("IsPressed", false);
        }
    }

    public void ResumeButton()
    {
        CloseOtherTabs();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        GlobalInspector.PlayerAlive = true;
        PauseMenu.SetActive(false);
        isPaused = false;
        playerGUI.enabled = true;
    }

    public void BestiaryButton()
    {
        CloseOtherTabs();
        BestiatyPanel.SetActive(true);
        Buttons.transform.Find("BestiaryButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void SettingsButton()
    {
        CloseOtherTabs();
        SettingsPanel.SetActive(true);
        Buttons.transform.Find("SettingsButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void DevelopersButton()
    {
        CloseOtherTabs();
        DevelopersPanel.SetActive(true);
        Buttons.transform.Find("DevelopersButton").gameObject.GetComponent<Animator>().SetBool("IsPressed", true);
    }

    public void ExitButton()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(0);
        GlobalInspector.PlayerAlive = true;
    }

    public void VolumeSlider(float value)
    {
        AudioListener.volume = value;
    }

    public void TakeDamage()
    {
        DamageIndicator.GetComponentInChildren<Animator>().SetTrigger("Damage");
    }
}

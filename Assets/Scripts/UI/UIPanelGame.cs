using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGame : MonoBehaviour,IMenu
{
    public Text TimeAttackTimer;
    [SerializeField] private GameObject TimerGO;
    [SerializeField] private Button btnPause;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnPause.onClick.AddListener(OnClickPause);
    }

    private void OnClickPause()
    {
        m_mngr.ShowPauseMenu();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    void Update()
    {
        UpdateTimerUI();
    }
    private void UpdateTimerUI()
    {
        if (m_mngr == null || m_mngr.GameManager == null || TimeAttackTimer == null) return;
        var gm = m_mngr.GameManager;
        bool ShowTimer = (gm.CurrentMode == GameManager.eLevelMode.TIME_ATTACK && gm.State == GameManager.eStateGame.GAME_STARTED);
        
        if (TimerGO != null && TimerGO.activeSelf != ShowTimer)
        {
            TimerGO.SetActive(ShowTimer);
        }

        if(TimeAttackTimer.gameObject.activeSelf != ShowTimer) TimeAttackTimer.gameObject.SetActive(ShowTimer);
        if (ShowTimer)
        {
            float remainingTime = Mathf.Max(0, gm.TimeAttackTimerValue);
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            TimeAttackTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }


    public void Show()
    {
        this.gameObject.SetActive(true);
        UpdateTimerUI();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}

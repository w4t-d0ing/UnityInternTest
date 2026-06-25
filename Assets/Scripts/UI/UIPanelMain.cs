using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    //[SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;
    [SerializeField] private Button btnAutoWin;
    [SerializeField] private Button btnAutoLose;
    [SerializeField] private Button btnTimeAttack;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnMoves.onClick.AddListener(OnClickMoves);
        //btnTimer.onClick.AddListener(OnClickTimer);
        if (btnAutoWin) btnAutoWin.onClick.AddListener(() => m_mngr.StartLevel(GameManager.eLevelMode.AUTO_WIN));
        if (btnAutoLose) btnAutoLose.onClick.AddListener(() => m_mngr.StartLevel(GameManager.eLevelMode.AUTO_LOSE));
        if (btnTimeAttack) btnTimeAttack.onClick.AddListener(() => m_mngr.StartLevel(GameManager.eLevelMode.TIME_ATTACK));
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        //if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnAutoWin) btnAutoWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
        if (btnTimeAttack) btnTimeAttack.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    // private void OnClickTimer()
    // {
    //     m_mngr.LoadLevelTimer();
    // }

    private void OnClickMoves()
    {
        m_mngr.StartLevel(GameManager.eLevelMode.NORMAL);
    }

    public void Show()=>this.gameObject.SetActive(true);


    public void Hide()=>this.gameObject.SetActive(false);

}

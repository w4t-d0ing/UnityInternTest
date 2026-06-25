using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        TIME_ATTACK,
        NORMAL,
        AUTO_WIN,
        AUTO_LOSE
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        WIN,
        LOSE
    }
    public eLevelMode CurrentMode { get; private set; }
    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }

    public BoardController BoardController => m_boardController;
    public float TimeAttackTimerValue => m_timeAttackTimer;

    private GameSettings m_gameSettings;
    private ContainerSettings m_containerSettings;

    private BoardController m_boardController;
    private ContainerManager m_containerManager;

    private UIMainManager m_uiMenu;

    // private LevelCondition m_levelCondition;

    private float m_timeAttackTimer;

    private void Awake()
    {
        State = eStateGame.SETUP;
        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);
        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();

        if (State == eStateGame.GAME_STARTED && CurrentMode == eLevelMode.TIME_ATTACK)
        {
            m_timeAttackTimer -= Time.deltaTime;
            
            // Optional: You can link m_timeAttackTimer to a UI Text element here!
            
            if (m_timeAttackTimer <= 0)
            {
                OnLose();
            }
        }
    }


    internal void SetState(eStateGame state)
    {
        State = state;
        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        CurrentMode = mode;

        if (CurrentMode == eLevelMode.TIME_ATTACK) m_timeAttackTimer = 60f;

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        Debug.Log("Current Mode: " + mode);
        m_boardController.StartGame(this, m_gameSettings, mode);

        m_containerManager = new GameObject("ContainerManager").AddComponent<ContainerManager>();
        m_containerManager.StartGame(this, m_gameSettings);

        m_boardController.SetUpContainerManager(m_containerManager);

        State = eStateGame.GAME_STARTED;
        #region Old LoadLevel Codes
        // if (mode == eLevelMode.MOVES)
        // {
        //     m_levelCondition = this.gameObject.AddComponent<LevelMoves>();
        //     m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
        // }
        // else if (mode == eLevelMode.TIMER)
        // {
        //     m_levelCondition = this.gameObject.AddComponent<LevelTime>();
        //     m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
        // }

        // m_levelCondition.ConditionCompleteEvent += GameOver;
        #endregion
    }

    public void OnWin()
    {
        if (State != eStateGame.GAME_STARTED) return;

        Debug.Log("Victory! The board is clear.");
        State = eStateGame.WIN;

        StartCoroutine(WaitBoardController());
    }


    public void OnLose()
    {
        if (State != eStateGame.GAME_STARTED) return;

        Debug.Log("Defeat! The container is full.");
        State = eStateGame.LOSE;

        StartCoroutine(WaitBoardController());
    }
    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        StopAllCoroutines();

        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        if (m_containerManager)
        {
            m_containerManager.Clear();
            Destroy(m_containerManager.gameObject);
            m_containerManager = null;
        }
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        // if (m_levelCondition != null)
        // {
        //     m_levelCondition.ConditionCompleteEvent -= GameOver;

        //     Destroy(m_levelCondition);
        //     m_levelCondition = null;
        // }
    }
}

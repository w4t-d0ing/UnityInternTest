using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;
    private ContainerManager m_containerManager;
    private GameSettings m_gameSettings;

    private Camera m_cam;
    private GameManager.eLevelMode m_currentMode;
    private bool m_gameOver;

    #region Old Variables
    //private bool m_isDragging;
    //private Collider2D m_hitCollider;
    // private List<Cell> m_potentialMatch;
    // private float m_timeAfterFill;
    // private bool m_hintIsShown;
    #endregion

    public void StartGame(GameManager gameManager, GameSettings gameSettings, GameManager.eLevelMode mode = GameManager.eLevelMode.NORMAL)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);
        m_currentMode = mode;

        Fill();
    }

    public void SetUpContainerManager(ContainerManager containerManager)
    {
        m_containerManager = containerManager;
        if (m_currentMode != GameManager.eLevelMode.NORMAL)
        {
            StartCoroutine(AutoRunRoutine());
        }
    }

    private IEnumerator AutoRunRoutine()
    {
        // Wait for board to settle
        yield return new WaitForSeconds(0.5f);

        while (!m_gameOver)
        {
            while (IsBusy) yield return null;
            
            yield return new WaitForSeconds(0.5f);

            if (m_gameOver) break;

            List<Cell> activeCells = m_board.GetAllCellsWithItem();
            if (activeCells.Count == 0) yield break;

            Cell targetCell = null;
            var containerCounts = m_containerManager.GetContainerItemCounts();

            if (m_currentMode == GameManager.eLevelMode.AUTO_WIN)
            {
                NormalItem.eNormalType? priorityType = null;
                int maxCount = 0;
                

                foreach (var kvp in containerCounts)
                {
                    if (kvp.Value > maxCount) { maxCount = kvp.Value; priorityType = kvp.Key; }
                }
                if (priorityType != null)
                {
                    targetCell = activeCells.Find(c => (c.Item as NormalItem).ItemType == priorityType);
                }
                if (targetCell == null) targetCell = activeCells[0];
            }
            else if (m_currentMode == GameManager.eLevelMode.AUTO_LOSE)
            {
                foreach (var cell in activeCells)
                {
                    var type = (cell.Item as NormalItem).ItemType;
                    int countInContainer = containerCounts.ContainsKey(type) ? containerCounts[type] : 0;
                    
                    if (countInContainer < 2) 
                    {
                        targetCell = cell;
                        break;
                    }
                }                
                if (targetCell == null) targetCell = activeCells[0]; 
            }
            if (targetCell != null)
            {
                OnCellTapped(targetCell);
            }
        }
    }

    private void Fill()
    {
        m_board.Fill();
        //FindMatchesAndCollapse();
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
            case GameManager.eStateGame.WIN:
            case GameManager.eStateGame.LOSE:
                m_gameOver = true;
                //StopHints();
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;


        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty)
                    OnCellTapped(cell);
            }

        }

        #region Old BoardController Update Code
        // if (!m_hintIsShown)
        // {
        //     m_timeAfterFill += Time.deltaTime;
        //     if (m_timeAfterFill > m_gameSettings.TimeForHint)
        //     {
        //         m_timeAfterFill = 0f;
        //         ShowHint();
        //     }
        // }

        // if (Input.GetMouseButtonDown(0))
        // {
        //     var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        //     if (hit.collider != null)
        //     {
        //         m_isDragging = true;
        //         m_hitCollider = hit.collider;
        //     }
        // }

        // if (Input.GetMouseButtonUp(0))
        // {
        //     ResetRayCast();
        // }

        // if (Input.GetMouseButton(0) && m_isDragging)
        // {
        //     var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        //     if (hit.collider != null)
        //     {
        //         if (m_hitCollider != null && m_hitCollider != hit.collider)
        //         {
        //             StopHints();

        //             Cell c1 = m_hitCollider.GetComponent<Cell>();
        //             Cell c2 = hit.collider.GetComponent<Cell>();
        //             if (AreItemsNeighbor(c1, c2))
        //             {
        //                 IsBusy = true;
        //                 SetSortingLayer(c1, c2);
        //                 m_board.Swap(c1, c2, () =>
        //                 {
        //                     FindMatchesAndCollapse(c1, c2);
        //                 });

        //                 ResetRayCast();
        //             }
        //         }
        //     }
        //     else
        //     {
        //         ResetRayCast();
        //     }
        // }

        #endregion
    }


    private void OnCellTapped(Cell cell)
    {
        m_containerManager.HandleClickedItem(cell);
        OnMoveEvent();
        IsBusy = true;

        Debug.Log("Clicked Cell");
        StartCoroutine(CheckAfterMoveCoroutine());
    }

    private IEnumerator CheckAfterMoveCoroutine()
    {
        yield return new WaitForSeconds(0.3f);

        if (m_gameManager.State == GameManager.eStateGame.LOSE)
        {
            IsBusy = false;
            yield break;
        }

        if (m_board.IsEmpty())
        {
            m_gameManager.OnWin();
        }
        IsBusy = false;
    }

    internal void Clear()
    {
        m_board.Clear();
    }

    #region Old Codes
    // private void ResetRayCast()
    // {
    //     m_isDragging = false;
    //     m_hitCollider = null;
    // }

    // private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    // {
    //     if (cell1.Item is BonusItem)
    //     {
    //         cell1.ExplodeItem();
    //         StartCoroutine(ShiftDownItemsCoroutine());
    //     }
    //     else if (cell2.Item is BonusItem)
    //     {
    //         cell2.ExplodeItem();
    //         StartCoroutine(ShiftDownItemsCoroutine());
    //     }
    //     else
    //     {
    //         List<Cell> cells1 = GetMatches(cell1);
    //         List<Cell> cells2 = GetMatches(cell2);

    //         List<Cell> matches = new List<Cell>();
    //         matches.AddRange(cells1);
    //         matches.AddRange(cells2);
    //         matches = matches.Distinct().ToList();

    //         if (matches.Count < m_gameSettings.MatchesMin)
    //         {
    //             m_board.Swap(cell1, cell2, () =>
    //             {
    //                 IsBusy = false;
    //             });
    //         }
    //         else
    //         {
    //             OnMoveEvent();

    //             CollapseMatches(matches, cell2);
    //         }
    //     }
    // }

    // private void FindMatchesAndCollapse()
    // {
    //     List<Cell> matches = m_board.FindFirstMatch();

    //     if (matches.Count > 0)
    //     {
    //         CollapseMatches(matches, null);
    //     }
    //     else
    //     {
    //         m_potentialMatch = m_board.GetPotentialMatches();
    //         if (m_potentialMatch.Count > 0)
    //         {
    //             IsBusy = false;

    //             m_timeAfterFill = 0f;
    //         }
    //         else
    //         {
    //             //StartCoroutine(RefillBoardCoroutine());
    //             StartCoroutine(ShuffleBoardCoroutine());
    //         }
    //     }
    // }

    // private List<Cell> GetMatches(Cell cell)
    // {
    //     List<Cell> listHor = m_board.GetHorizontalMatches(cell);
    //     if (listHor.Count < m_gameSettings.MatchesMin)
    //     {
    //         listHor.Clear();
    //     }

    //     List<Cell> listVert = m_board.GetVerticalMatches(cell);
    //     if (listVert.Count < m_gameSettings.MatchesMin)
    //     {
    //         listVert.Clear();
    //     }

    //     return listHor.Concat(listVert).Distinct().ToList();
    // }

    // private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    // {
    //     for (int i = 0; i < matches.Count; i++)
    //     {
    //         matches[i].ExplodeItem();
    //     }

    //     if (matches.Count > m_gameSettings.MatchesMin)
    //     {
    //         m_board.ConvertNormalToBonus(matches, cellEnd);
    //     }

    //     StartCoroutine(ShiftDownItemsCoroutine());
    // }

    // private IEnumerator ShiftDownItemsCoroutine()
    // {
    //     m_board.ShiftDownItems();

    //     yield return new WaitForSeconds(0.2f);

    //     m_board.FillGapsWithNewItems();

    //     yield return new WaitForSeconds(0.2f);

    //     FindMatchesAndCollapse();
    // }

    // private IEnumerator RefillBoardCoroutine()
    // {
    //     m_board.ExplodeAllItems();

    //     yield return new WaitForSeconds(0.2f);

    //     m_board.Fill();

    //     yield return new WaitForSeconds(0.2f);

    //     FindMatchesAndCollapse();
    // }

    // private IEnumerator ShuffleBoardCoroutine()
    // {
    //     m_board.Shuffle();

    //     yield return new WaitForSeconds(0.3f);

    //     FindMatchesAndCollapse();
    // }


    // private void SetSortingLayer(Cell cell1, Cell cell2)
    // {
    //     if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
    //     if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    // }

    // private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    // {
    //     return cell1.IsNeighbour(cell2);
    // }



    // private void ShowHint()
    // {
    //     m_hintIsShown = true;
    //     foreach (var cell in m_potentialMatch)
    //     {
    //         cell.AnimateItemForHint();
    //     }
    // }

    // private void StopHints()
    // {
    //     m_hintIsShown = false;
    //     foreach (var cell in m_potentialMatch)
    //     {
    //         cell.StopHintAnimation();
    //     }

    //     m_potentialMatch.Clear();
    // }
    #endregion
}

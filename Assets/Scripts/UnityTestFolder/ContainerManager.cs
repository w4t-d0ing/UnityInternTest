using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ContainerManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Vector3 m_startPos;

    private Container m_ContainerBoard;
    private BoardController m_boardController;
    private GameManager m_gameManager;

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_boardController = gameManager.BoardController;

        m_ContainerBoard = new Container(
            transform,
            gameSettings.ContainerSettings,
            gameSettings.ContainerStartPos
        );
    }

    public Dictionary<NormalItem.eNormalType, int> GetContainerItemCounts() 
    {
        return m_ContainerBoard.GetItemCounts();
    }
    
    public void HandleClickedItem(Cell currentCell)
    {
        if (m_boardController.IsBusy) { Debug.Log("boardIsBusy"); return; }
        if (m_ContainerBoard.isFull) { Debug.Log("Container Full"); return; }

        Item item = currentCell.Item;
        if (item == null) return;

        if (currentCell.IsContainerCell)
        {
            if (m_gameManager.CurrentMode == GameManager.eLevelMode.TIME_ATTACK)
            {
                Cell targetBoardCell = item.OriginalCell;
                if (targetBoardCell == null || !targetBoardCell.IsEmpty) return; // Safety check

                m_ContainerBoard.RemoveItem(currentCell);
                targetBoardCell.Assign(item);
                
                item.SetViewRoot(m_boardController.transform); 
                item.View.DOMove(targetBoardCell.transform.position, 0.3f);
            }
            return; 
        }

        if (m_ContainerBoard.isFull) return;
        item.OriginalCell = currentCell;
        currentCell.Free();
        item.SetViewRoot(null);

        m_ContainerBoard.TryAddItem(item, () => 
        {
            if (m_ContainerBoard.isFull && m_gameManager.CurrentMode != GameManager.eLevelMode.TIME_ATTACK)
            {
                m_gameManager.OnLose();
            }
        });

    }

    private void OnDestroy()
    {
        if(m_boardController != null) Destroy(m_boardController);
    }

    public void Clear() => m_ContainerBoard?.Clear();
}
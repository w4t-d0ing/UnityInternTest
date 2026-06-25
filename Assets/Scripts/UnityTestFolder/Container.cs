using DG.Tweening;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Container
{
    // private ContainerSettings containerSettings;
    [SerializeField]private Cell[] m_cells;
    private Transform m_transform;
    private int m_boardCapacity;
    private int m_matchMin;
    private Vector3 startPos;
    public bool isFull
    {
        get
        {
            foreach (var n in m_cells)
            {
                if (n.IsEmpty) return false;
            }
            return true;
        }
    }
    public Container(Transform transform, ContainerSettings containerSettings,Vector3 startPos)
    {
        m_cells = new Cell[containerSettings.Capacity];
        m_boardCapacity = containerSettings.Capacity;
        m_transform = transform;
        this.startPos = startPos;
        m_matchMin = containerSettings.Match_Min;
        CreateContainer();
    }

    void CreateContainer()
    {
        GameObject prefab = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        
        for(int i = 0; i< m_boardCapacity; i++)
        {
            Vector3 pos = startPos + Vector3.right * i;
            GameObject go = GameObject.Instantiate(prefab, pos, Quaternion.identity, m_transform);
            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i,0);
            cell.IsContainerCell = true;
            m_cells[i] = cell;
        }


        for (int i = 0 ; i < m_boardCapacity ; i++)
        {
            if( i + 1 < m_boardCapacity) m_cells[i].NeighbourRight = m_cells [i+ 1];
            if( i > 0 ) m_cells[i].NeighbourLeft = m_cells[i-1]; 
        }
    }

    public Dictionary<NormalItem.eNormalType, int> GetItemCounts()
    {
        var counts = new Dictionary<NormalItem.eNormalType, int>();
        foreach (var cell in m_cells)
        {
            if (!cell.IsEmpty && cell.Item is NormalItem normalItem)
            {
                if (!counts.ContainsKey(normalItem.ItemType)) 
                    counts[normalItem.ItemType] = 0;
                
                counts[normalItem.ItemType]++;
            }
        }
        return counts;
    }

    public void ClearMatches(List<Cell> matchingCells)
    {
        foreach (var n in matchingCells) n.ExplodeItem();
        
    }

    public void ShiftItemLeft()
    {
        int shifts = 0;
        for (int x = 0; x < m_boardCapacity; x++)
            {
                Cell cell = m_cells[x];
                if (cell.IsEmpty)
                {
                    shifts++;
                    continue;
                }

                if (shifts == 0) continue;

                Cell holder = m_cells[x - shifts];

                Item item = cell.Item;
                cell.Free();

                holder.Assign(item);
                item.View.DOMove(holder.transform.position, 0.3f);
            }
    }
    public bool TryAddItem(Item item, Action onComplete = null)
    {
        int firstEmptyIndex = -1;
        for (int i = 0; i < m_boardCapacity; i++)
        {
            if (m_cells[i].IsEmpty)
            {
                firstEmptyIndex = i;
                break;
            }
        }

        if(firstEmptyIndex == -1) return false;

        int insertIndex = firstEmptyIndex;

        for (int i = firstEmptyIndex - 1; i >= 0; i--)
        {
            if (!m_cells[i].IsEmpty && m_cells[i].Item.IsSameType(item))
            {
                insertIndex = i + 1;
                break;
            }
        }

        ShiftContainerItem(firstEmptyIndex, insertIndex);

        Cell targetCell = m_cells[insertIndex];
        Vector3 targetPos = targetCell.transform.position;
        targetCell.Assign(item);

        item.View
            .DOMove(targetPos, 0.3f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                item.SetViewRoot(m_transform);
                item.SetViewPosition(targetPos);             
                CheckAndClearMatches();
                onComplete?.Invoke();
            });

        return true;
    }

    public void RemoveItem(Cell cellToRemove)
    {
        int index = Array.IndexOf(m_cells, cellToRemove);
        if (index == -1) return;

        cellToRemove.Free();
        for (int i = index; i < m_boardCapacity - 1; i++)
        {
            Cell nextCell = m_cells[i + 1];
            if (!nextCell.IsEmpty)
            {
                Item itemToShift = nextCell.Item;
                nextCell.Free();
                m_cells[i].Assign(itemToShift);
                itemToShift.View.DOMove(m_cells[i].transform.position, 0.2f);
            }
        }
    }

    private void ShiftContainerItem(int firstEmptyIndex, int insertIndex)
    {
        for (int i = firstEmptyIndex; i > insertIndex; i--)
            {
                Cell currentCell = m_cells[i - 1];
                Cell nextCell = m_cells[i];

                Item itemToShift = currentCell.Item;
                
                currentCell.Free();
                nextCell.Assign(itemToShift);

                itemToShift.View
                    .DOMove(nextCell.transform.position, 0.3f)
                    .SetEase(Ease.OutQuad);
            }
    }

    private void CheckAndClearMatches()
    {
        List<Cell> matches = FindMatches();
        if (matches.Count > 0)
        {
            Debug.Log($"[Container] Match found — clearing {matches.Count} items");
            ClearMatches(matches);
            ShiftItemLeft();
        }
    }

    private List<Cell> FindMatches()
    {
        for(int i = 0; i< m_boardCapacity ; i++)
        {
            if(m_cells[i].IsEmpty) continue;

            List<Cell> matchingCells = GetHorizontalMatches(m_cells[i]);
            if(matchingCells.Count >= m_matchMin) return matchingCells;
        }
        return new List<Cell>();
    }

    private List<Cell> GetHorizontalMatches(Cell cell)
    {
        List<Cell> list = new List<Cell>();

        Cell current = cell;
        list.Add(cell);
        while (true)
        {
            Cell neib = current.NeighbourRight;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                current = neib;
            }
            else break;
        }

        current = cell;
        while (true)
        {
            Cell neib = current.NeighbourLeft;
            if (neib == null) break;

            if (neib.IsSameType(cell))
            {
                list.Add(neib);
                current = neib;
            }
            else break;
        }

        return list;
    }
    public void Clear()
    {
        foreach (var n in m_cells)
        {
            n.Clear();
            GameObject.Destroy(n.gameObject);
        }
    }
}

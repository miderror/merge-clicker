using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, ISaveSystem
{
    [SerializeField] private Pickaxe _pickaxePrefab = null;
    [SerializeField] private Cell[] _cells = null;

    private List<Pickaxe> _pickaxes = new List<Pickaxe>();

    void Start()
    {
        if (_cells == null || _cells.Length == 0)
        {
            _cells = GetComponentsInChildren<Cell>();
        }
        foreach (Cell cell in _cells)
        {
            cell.onDestroyPickaxe += UpdatePickaxes;
        }

        SaveManager.Instance.RegisterSaveSystem(this);

        UIManager.UpdateBuyButtonText(GetNextCostOfPickaxe());
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnregisterSaveSystem(this);
        }
    }

    public bool AddPickaxe(int level = 0)
    {
        List<Cell> emptyCells = GetEmptyCells();

        if (emptyCells.Count == 0)
        {
            return false;
        }

        Cell randomCell = emptyCells[Random.Range(0, emptyCells.Count)];

        Pickaxe newPickaxe = Instantiate(_pickaxePrefab, randomCell.transform);
        newPickaxe.SetPickaxe(level);

        randomCell.SetPickaxe(newPickaxe);
        _pickaxes.Add(newPickaxe);

        return true;
    }

    public List<Cell> GetEmptyCells()
    {
        List<Cell> emptyCells = new List<Cell>();

        foreach (Cell cell in _cells)
        {
            if (!cell.HasPickaxe())
            {
                emptyCells.Add(cell);
            }
        }

        return emptyCells;
    }

    public List<Cell> GetOccupiedCells()
    {
        List<Cell> occupiedCells = new List<Cell>();

        foreach (Cell cell in _cells)
        {
            if (cell.HasPickaxe())
            {
                occupiedCells.Add(cell);
            }
        }

        return occupiedCells;
    }

    public int GetPickaxeCount()
    {
        int count = 0;
        foreach (Cell cell in _cells)
        {
            Pickaxe pickaxe = cell.GetPickaxe();
            if (pickaxe != null)
            {
                count += pickaxe.GetLvl() + 1;
            }
        }
        return count;
    }

    public void SaveData()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            Pickaxe pickaxe = _cells[i].GetPickaxe();

            if (pickaxe != null)
            {
                print(i);
                PlayerPrefs.SetInt($"cell{i}", pickaxe.GetLvl());
            }
        }
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        ClearInventory();

        for (int i = 0; i < _cells.Length; i++)
        {
            if (PlayerPrefs.HasKey($"cell{i}"))
            {
                print(i);
                Pickaxe newPickaxe = Instantiate(_pickaxePrefab, _cells[i].transform);
                newPickaxe.SetPickaxe(PlayerPrefs.GetInt($"cell{i}"));

                _cells[i].SetPickaxe(newPickaxe);
                _pickaxes.Add(newPickaxe);
            }
        }
    }

    public void ClearInventory()
    {
        foreach (Cell cell in _cells)
        {
            Pickaxe pickaxe = cell.GetPickaxe();
            if (pickaxe != null)
            {
                Destroy(pickaxe.gameObject);
            }
        }

        _pickaxes.Clear();
    }

    public void DropAll()
    {
        List<Cell> cells = GetOccupiedCells();
        if (cells.Count != _pickaxes.Count)
        {
            return;
        }
        foreach (Cell cell in cells)
        {
            cell.GetPickaxe().StartAttack();
        }
    }

    public int GetNextCostOfPickaxe()
    {
        return GetPickaxeCount() * 10;
    }

    public void BuyPickaxe()
    {
        if (GetOccupiedCells().Count != _pickaxes.Count)
        {
            return;
        }
        int cost = GetNextCostOfPickaxe();
        if (GameManager.SpendMoney(cost))
        {
            AddPickaxe();
            UIManager.UpdateBuyButtonText(GetNextCostOfPickaxe());
        }
    }

    private void UpdatePickaxes(Pickaxe pickaxe)
    {
        _pickaxes.Remove(pickaxe);
        foreach (Cell cell in _cells)
        {
            cell.UpdatePickaxeState();
        }
    }
}
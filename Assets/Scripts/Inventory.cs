using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour, ISaveSystem
{
    [SerializeField] private Pickaxe _pickaxePrefab = null;
    [SerializeField] private Cell[] _cells = null;

    private List<Pickaxe> _pickaxes = new List<Pickaxe>();


    private void Awake()
    {
        SaveManager.Instance.RegisterSaveSystem(this);
    }

    private void Start()
    {
        if (_cells == null || _cells.Length == 0)
        {
            _cells = GetComponentsInChildren<Cell>();
        }
        foreach (Cell cell in _cells)
        {
            cell.onDestroyPickaxe += UpdatePickaxes;
        }

        UIManager.UpdateBuyButtonText(GetNextCostOfPickaxe());
    }

    private void OnDestroy()
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
                count += (int)Mathf.Pow(2, pickaxe.GetLvl());
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
                PlayerPrefs.SetInt($"cell{i}", pickaxe.GetLvl());
            }
            else
            {
                PlayerPrefs.SetInt($"cell{i}", -1);
            }
        }
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        ClearInventory();

        for (int i = 0; i < _cells.Length; i++)
        {
            if (PlayerPrefs.GetInt($"cell{i}", -1) >= 0)
            {
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
        foreach (Cell cell in cells)
        {
            if (cell.GetPickaxe().IsFalling())
            {
                return;
            }
        }
        foreach (Cell cell in cells)
        {
            cell.GetPickaxe().StartAttack();
        }
    }

    public int GetNextCostOfPickaxe()
    {
        return GetPickaxeCount();
    }

    public void BuyPickaxe()
    {
        List<Cell> cells = GetOccupiedCells();
        foreach (Cell cell in cells)
        {
            if (cell.GetPickaxe().IsFalling())
            {
                return;
            }
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
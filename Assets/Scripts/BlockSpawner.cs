using System;
using UnityEngine;

public class BlockSpawner : MonoBehaviour, ISaveSystem
{
    [Tooltip("In order of increasing the health of the blocks")]
    [SerializeField] private Block[] _blocks = null;
    [SerializeField] private int _blocksCount = 18;

    private Block[] _lvlBlocks = null;

    private const int SeedLvlGeneration = 416;  // Any number. Necessary for the determinism of the random function

    public Action onDiggedAll = null;


    private void Awake()
    {
        SaveManager.Instance.RegisterSaveSystem(this);
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnregisterSaveSystem(this);
        }
    }

    public void GenerateBlocks()
    {
        ClearBlocks();

        _lvlBlocks = new Block[_blocksCount];

        for (int i = 0; i < _lvlBlocks.Length; ++i)
        {
            var random = new System.Random(SeedLvlGeneration + _blocksCount * GameManager.CurrentLevel + i);
            int blockIndex = Mathf.Clamp(random.Next(GameManager.CurrentLevel / 3 - 2, GameManager.CurrentLevel / 3 + 1), 0, _blocks.Length - 1);

            _lvlBlocks[i] = Instantiate(_blocks[blockIndex], transform);
            _lvlBlocks[i].onDigged += CheckBlocks;
        }
    }

    private void ClearBlocks()
    {
        if (_lvlBlocks == null) 
        { 
            return; 
        }

        foreach (Block block in _lvlBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        _lvlBlocks = null;
    }


    private void CheckBlocks()
    {
        foreach(Block block in _lvlBlocks)
        {
            if (block.curHealth > 0)
            {
                return;
            }
        }
        onDiggedAll?.Invoke();
    }

    public Block GetBlockAtPosition(Vector3 worldPosition, float tolerance = 50f)
    {
        if (_lvlBlocks == null) return null;

        Block closestBlock = null;
        float closestDistance = float.MaxValue;

        foreach (Block block in _lvlBlocks)
        {
            if (block.curHealth > 0 && block.gameObject.activeInHierarchy)
            {
                float distance = Vector3.Distance(worldPosition, block.transform.position);

                if (distance < tolerance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }

        return closestBlock;
    }

    public void SaveData()
    {
        if (_lvlBlocks == null)
        {
            return;
        }
        for (int i = 0; i < _lvlBlocks.Length; ++i)
        {
            PlayerPrefs.SetInt($"block{i}", _lvlBlocks[i].curHealth);
        }
    }

    public void LoadData()
    {
        GenerateBlocks();
        for (int i = 0; i < _lvlBlocks.Length; ++i)
        {
            if (PlayerPrefs.HasKey($"block{i}"))
            {
                _lvlBlocks[i].curHealth = PlayerPrefs.GetInt($"block{i}");
                if (_lvlBlocks[i].curHealth <= 0)
                {
                    _lvlBlocks[i].Deactive();
                }
            }
        }
    }
}

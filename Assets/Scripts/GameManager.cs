using UnityEngine;

public class GameManager : MonoBehaviour, ISaveSystem
{
    [SerializeField] private BlockSpawner blockSpawner;

    public static int CurrentLevel { get; private set; } = 1;
    public static int PlayerMoney { get; private set; } = 0;

    public static System.Action<int> OnMoneyChanged;
    public static System.Action<int> OnLevelChanged;

    private static GameManager instance = null;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("GameManager");
                instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        SaveManager.Instance.RegisterSaveSystem(this);
        SaveManager.Instance.LoadAllData();

        GenerateLevel(CurrentLevel);
    }

    void Start()
    {
        if (blockSpawner != null)
        {
            blockSpawner.onDiggedAll += OnLevelCompleted;
        }
    }

    void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UnregisterSaveSystem(this);
        }

        if (blockSpawner != null)
        {
            blockSpawner.onDiggedAll -= OnLevelCompleted;
        }
    }

    public void GenerateLevel(int level)
    {
        CurrentLevel = level;
        OnLevelChanged?.Invoke(level);
        blockSpawner.GenerateBlocks();
    }

    public void NextLevel()
    {
        GenerateLevel(CurrentLevel + 1);
    }

    private void OnLevelCompleted()
    {
        Invoke(nameof(NextLevel), 2f);
    }

    public static void AddMoney(int amount)
    {
        PlayerMoney += amount;
        OnMoneyChanged?.Invoke(PlayerMoney);
    }

    public static bool SpendMoney(int amount)
    {
        if (PlayerMoney >= amount)
        {
            PlayerMoney -= amount;
            OnMoneyChanged?.Invoke(PlayerMoney);
            return true;
        }
        return false;
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("LVL", CurrentLevel);
        PlayerPrefs.SetInt("Money", PlayerMoney);
    }

    public void LoadData()
    {
        CurrentLevel = PlayerPrefs.GetInt("LVL", 1);
        PlayerMoney = PlayerPrefs.GetInt("Money", 0);

        OnLevelChanged?.Invoke(CurrentLevel);
        OnMoneyChanged?.Invoke(PlayerMoney);
    }
}

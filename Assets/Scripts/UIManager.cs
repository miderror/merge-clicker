using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _buyButtonText = null;
    [SerializeField] private TextMeshProUGUI _moneyText = null;
    [SerializeField] private TextMeshProUGUI _lvlText = null;

    private static UIManager _instance;

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("UIManager");
                    _instance = obj.AddComponent<UIManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameManager.OnMoneyChanged += UpdateMoneyText;
        GameManager.OnLevelChanged += UpdateLvlText;

        if (GameManager.Instance != null)
        {
            UpdateMoneyText(GameManager.PlayerMoney);
            UpdateLvlText(GameManager.CurrentLevel);
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            GameManager.OnMoneyChanged -= UpdateMoneyText;
        }
    }

    public void UpdateMoneyText(int money)
    {
        _moneyText.text = $"{money} $";
    }

    public void UpdateLvlText(int lvl)
    {
        _lvlText.text = $"LVL {lvl}";
    }

    public static void UpdateBuyButtonText(int money)
    {
        Instance._buyButtonText.text = $"{money} $";
    }
}
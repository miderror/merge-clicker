using System;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    [Header("General Properties")]
    [SerializeField] private int _maxHealth = 5;
    [SerializeField] private int _reward = 5;
    [SerializeField] private Sprite[] _destroyStates = null;
    [Space]
    [Header("Components")]
    [SerializeField] private Image _blockImage = null;
    [SerializeField] private Image _destroyImage = null;

    public Action onDigged = null;

    private int _curHealth = 0;
    public int curHealth
    {
        get 
        { 
            return _curHealth; 
        }
        set 
        { 
            _curHealth = value;
            UpdateDestroyState();
        }
    }


    private void Awake()
    {
        _curHealth = _maxHealth;
    }

    public void Dig(int damage)
    {
        _curHealth = Math.Max(0, _curHealth - damage);
        GameManager.AddMoney(damage);
        UpdateDestroyState();

        if (_curHealth <= 0)
        {
            Digged();
        }
    }

    private void UpdateDestroyState()
    {
        float healthPercent = (_maxHealth - _curHealth) / (float)_maxHealth;
        int state = Mathf.FloorToInt(healthPercent * _destroyStates.Length);
        state = Mathf.Clamp(state, 0, _destroyStates.Length - 1);

        if (state == 0 || _curHealth <= 0)
        {
            _destroyImage.sprite = null;
            _destroyImage.color = new Color(0, 0, 0, 0);
        }
        else if (state < _destroyStates.Length) 
        {
            _destroyImage.sprite = _destroyStates[state - 1];
            _destroyImage.color = Color.white;
        }
    }

    private void Digged()
    {
        GameManager.AddMoney(_reward);
        onDigged?.Invoke();
        Deactive();
    }

    public void Deactive()
    {
        _blockImage.color = new Color(0, 0, 0, 0);
        _destroyImage.sprite = null;
        _destroyImage.color = new Color(0, 0, 0, 0);
    }
}

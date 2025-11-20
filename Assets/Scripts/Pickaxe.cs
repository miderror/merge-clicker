using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class Pickaxe : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform _pickaxeRect = null;
    [SerializeField] private Image _pickaxeImage = null;

    [Header("Animation Settings")]
    [SerializeField] private float _fallSpeed = 200f;
    [SerializeField] private float _bounceHeight = 30f;
    [SerializeField] private float _bounceDuration = 0.2f;
    [SerializeField] private float _rotationSpeed = 360f;

    [Header("Pickaxe Properties")]
    [SerializeField] private int _maxDurability = 10;
    [SerializeField] private int _damage = 1;

    private int _currentDurability = 0;
    private bool _isFalling = false;
    private bool _isBouncing = false;
    private Block _currentTargetBlock = null;
    private Transform _originalParent;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public System.Action OnPickaxeBreak = null;
    public System.Action<int> OnDurabilityChanged = null;

    void Start()
    {
        _originalParent = _pickaxeRect.parent;
        _originalPosition = _pickaxeRect.anchoredPosition;
        _originalRotation = _pickaxeRect.rotation;
        _currentDurability = _maxDurability;
    }

    [ContextMenu("Attack")]
    public void StartAttack()
    {
        if (_isFalling || _currentDurability <= 0)
        {
            return;
        }

        _pickaxeRect.SetParent(_originalParent.parent.parent, true);
        _isFalling = true;

        StartCoroutine(FallingRoutine());
    }

    private IEnumerator FallingRoutine()
    {
        while (_isFalling && _currentDurability > 0)
        {
            _pickaxeRect.Rotate(0, 0, _rotationSpeed * Time.deltaTime);

            Vector3 newPosition = _pickaxeRect.anchoredPosition + Vector2.down * _fallSpeed * Time.deltaTime;
            _pickaxeRect.anchoredPosition = newPosition;

            Block hitBlock = CheckBlockCollision();

            if (hitBlock != null && hitBlock.curHealth > 0)
            {
                if (_currentTargetBlock != hitBlock)
                {
                    _currentTargetBlock = hitBlock;
                    yield return StartCoroutine(HitBlockRoutine());
                }
            }

            if (_pickaxeRect.anchoredPosition.y < -Screen.height / 2)
            {
                yield return StartCoroutine(ReturnToStartPosition());
                break;
            }

            yield return null;
        }
    }

    private IEnumerator HitBlockRoutine()
    {
        if (_currentTargetBlock == null || _currentTargetBlock.curHealth <= 0) yield break;

        _isBouncing = true;

        Vector3 hitPosition = _pickaxeRect.anchoredPosition;

        Vector3 bouncePosition = hitPosition + Vector3.up * _bounceHeight;
        _pickaxeRect.DOAnchorPos(bouncePosition, _bounceDuration / 2);
        yield return new WaitForSeconds(_bounceDuration / 2);

        _currentTargetBlock.Dig(_damage);
        _currentDurability--;
        OnDurabilityChanged?.Invoke(_currentDurability);

        if (_currentDurability <= 0)
        {
            OnPickaxeBreak?.Invoke();
            yield return StartCoroutine(ReturnToStartPosition());
            yield break;
        }

        if (_currentTargetBlock.curHealth <= 0)
        {
            _currentTargetBlock = null;
        }
        else
        {
            while (_currentTargetBlock != null && _currentTargetBlock.curHealth > 0 && _currentDurability > 0)
            {
                _pickaxeRect.DOAnchorPos(hitPosition, _bounceDuration / 2);
                yield return new WaitForSeconds(_bounceDuration / 2);

                _currentTargetBlock.Dig(_damage);
                _currentDurability--;
                OnDurabilityChanged?.Invoke(_currentDurability);

                if (_currentDurability <= 0)
                {
                    OnPickaxeBreak?.Invoke();
                    yield return StartCoroutine(ReturnToStartPosition());
                    yield break;
                }

                if (_currentTargetBlock.curHealth <= 0)
                {
                    _currentTargetBlock = null;
                    break;
                }

                _pickaxeRect.DOAnchorPos(bouncePosition, _bounceDuration / 2);
                yield return new WaitForSeconds(_bounceDuration / 2);
            }
        }

        _isBouncing = false;
    }


    private Block CheckBlockCollision()
    {
        Block[] allBlocks = FindObjectsOfType<Block>();
        Block closestBlock = null;
        float closestDistance = float.MaxValue;

        Vector3 pickaxeWorldPos = _pickaxeRect.position;
        Vector2 pickaxeSize = GetPickaxeWorldSize();

        foreach (Block block in allBlocks)
        {
            if (block.curHealth > 0 && block.gameObject.activeInHierarchy)
            {
                RectTransform blockRect = block.GetComponent<RectTransform>();
                if (blockRect == null)
                {
                    continue;
                }

                Vector3 blockWorldPos = blockRect.position;
                Vector2 blockSize = GetBlockWorldSize(blockRect);

                float pickaxeBottom = pickaxeWorldPos.y - pickaxeSize.y / 2;
                float pickaxeLeft = pickaxeWorldPos.x - pickaxeSize.x / 2;
                float pickaxeRight = pickaxeWorldPos.x + pickaxeSize.x / 2;

                float blockTop = blockWorldPos.y + blockSize.y / 2;
                float blockBottom = blockWorldPos.y - blockSize.y / 2;
                float blockLeft = blockWorldPos.x - blockSize.x / 2;
                float blockRight = blockWorldPos.x + blockSize.x / 2;

                bool yCollision = pickaxeBottom <= blockTop && pickaxeBottom >= blockBottom;

                bool xCollision = pickaxeRight > blockLeft && pickaxeLeft < blockRight;

                if (yCollision && xCollision)
                {
                    float distance = Mathf.Abs(pickaxeBottom - blockTop);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestBlock = block;
                    }
                }
            }
        }

        return closestBlock;
    }

    private Vector2 GetPickaxeWorldSize()
    {
        Vector2 size = _pickaxeRect.rect.size;
        Vector3 worldScale = _pickaxeRect.lossyScale;
        return new Vector2(size.x * worldScale.x, size.y * worldScale.y);
    }

    private Vector2 GetBlockWorldSize(RectTransform blockRect)
    {
        Vector2 size = blockRect.rect.size;
        Vector3 worldScale = blockRect.lossyScale;
        return new Vector2(size.x * worldScale.x, size.y * worldScale.y);
    }

    private IEnumerator ReturnToStartPosition()
    {
        _isFalling = false;
        _isBouncing = false;

        _pickaxeRect.SetParent(_originalParent, false);

        _pickaxeRect.DOAnchorPos(_originalPosition, 0.5f);
        _pickaxeRect.DORotateQuaternion(_originalRotation, 0.5f);

        yield return new WaitForSeconds(0.5f);

        _currentTargetBlock = null;
        ResetPickaxe();
    }

    public void StopAttack()
    {
        if (_isFalling)
        {
            StartCoroutine(ReturnToStartPosition());
        }
    }

    public void ResetPickaxe()
    {
        StopAllCoroutines();

        _pickaxeRect.SetParent(_originalParent, false);
        _pickaxeRect.anchoredPosition = _originalPosition;
        _pickaxeRect.rotation = _originalRotation;

        _currentDurability = _maxDurability;
        _isFalling = false;
        _isBouncing = false;
        _currentTargetBlock = null;

        OnDurabilityChanged?.Invoke(_currentDurability);
    }

    public void UpgradePickaxe(int extraDurability, int extraDamage)
    {
        _maxDurability += extraDurability;
        _damage += extraDamage;
        ResetPickaxe();
    }

    public bool IsFalling() => _isFalling;
    public bool IsBouncing() => _isBouncing;
    public int GetCurrentDurability() => _currentDurability;
    public int GetMaxDurability() => _maxDurability;
}
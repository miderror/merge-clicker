using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler
{
    public Action<Pickaxe> onDestroyPickaxe = null;
    private Pickaxe _currentPickaxe = null;

    private static Pickaxe _draggedPickaxe = null;
    private static Cell _dragStartCell = null;
    private Canvas _canvas;

    void Start()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (_currentPickaxe != null && _draggedPickaxe == null)
        {
            StartDrag();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (_draggedPickaxe != null)
        {
            EndDrag();
        }
    }

    private void StartDrag()
    {
        _draggedPickaxe = _currentPickaxe;
        _dragStartCell = this;

        Image pickaxeImage = _draggedPickaxe.GetComponent<Image>();
        if (pickaxeImage != null)
        {
            pickaxeImage.raycastTarget = false;
        }

        _currentPickaxe.transform.SetParent(transform.root, true);
        _currentPickaxe = null;

        UpdateDraggedPosition();

        StartCoroutine(DragCoroutine());
    }

    private IEnumerator DragCoroutine()
    {
        while (_draggedPickaxe != null)
        {
            UpdateDraggedPosition();
            yield return null;
        }
    }

    private Vector2 GetPointerPosition()
    {
        if (Touchscreen.current != null && UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].screenPosition;
        }
        else if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    private void UpdateDraggedPosition()
    {
        if (_draggedPickaxe == null)
        {
            return;
        }

        RectTransform pickaxeRect = _draggedPickaxe.GetComponent<RectTransform>();
        if (pickaxeRect == null)
        {
            return;
        }

        Vector2 pointerPosition = GetPointerPosition();

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            pointerPosition,
            _canvas.worldCamera,
            out localPoint))
        {
            pickaxeRect.anchoredPosition = localPoint;
        }
    }

    private void EndDrag()
    {
        if (_draggedPickaxe != null)
        {
            Image pickaxeImage = _draggedPickaxe.GetComponent<Image>();
            if (pickaxeImage != null)
            {
                pickaxeImage.raycastTarget = true;
            }

            RectTransform pickaxeRect = _draggedPickaxe.GetComponent<RectTransform>();
            if (pickaxeRect != null)
            {
                pickaxeRect.localScale = Vector3.one;
            }
        }

        Cell targetCell = GetCellUnderCursor();

        if (targetCell != null)
        {
            HandleDropOnCell(targetCell);
        }
        else
        {
            ReturnPickaxeToStartCell();
        }

        ResetDragState();
    }

    private void HandleDropOnCell(Cell targetCell)
    {
        if (targetCell._currentPickaxe == null)
        {
            targetCell.SetPickaxe(_draggedPickaxe);
        }
        else
        {
            Pickaxe targetPickaxe = targetCell._currentPickaxe;

            if (targetPickaxe.GetLvl() == _draggedPickaxe.GetLvl())
            {
                bool upgradeSuccess = targetPickaxe.UpgradePickaxe();

                if (upgradeSuccess)
                {
                    onDestroyPickaxe?.Invoke(_draggedPickaxe);
                    Destroy(_draggedPickaxe.gameObject);
                }
                else
                {
                    ReturnPickaxeToStartCell();
                }
            }
            else
            {
                ReturnPickaxeToStartCell();
            }
        }
    }

    private void ReturnPickaxeToStartCell()
    {
        if (_dragStartCell != null)
        {
            _dragStartCell.SetPickaxe(_draggedPickaxe);
        }
        else
        {
            onDestroyPickaxe?.Invoke(_draggedPickaxe);
            Destroy(_draggedPickaxe.gameObject);
        }
    }

    private void ResetDragState()
    {
        _draggedPickaxe = null;
        _dragStartCell = null;
    }

    private Cell GetCellUnderCursor()
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        Vector2 pointerPosition = GetPointerPosition();

        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        }, results);

        foreach (var result in results)
        {
            Cell cell = result.gameObject.GetComponent<Cell>();
            if (cell != null && cell != _dragStartCell)
            {
                return cell;
            }
        }

        return null;
    }

    public void SetPickaxe(Pickaxe pickaxe)
    {
        _currentPickaxe = pickaxe;

        if (pickaxe != null)
        {
            pickaxe.transform.SetParent(transform, false);

            RectTransform pickaxeRect = pickaxe.GetComponent<RectTransform>();
            if (pickaxeRect != null)
            {
                pickaxeRect.anchoredPosition = Vector2.zero;
                pickaxeRect.rotation = Quaternion.identity;
                pickaxeRect.localScale = Vector3.one;
            }

            Image pickaxeImage = pickaxe.GetComponent<Image>();
            if (pickaxeImage != null)
            {
                pickaxeImage.raycastTarget = true;
            }

            pickaxe.StopAttack();
        }
    }

    public bool HasPickaxe()
    {
        return _currentPickaxe != null;
    }

    public Pickaxe GetPickaxe()
    {
        return _currentPickaxe;
    }

    public void InitializeWithPickaxe(Pickaxe pickaxePrefab, int level)
    {
        if (pickaxePrefab != null)
        {
            Pickaxe newPickaxe = Instantiate(pickaxePrefab, transform);
            newPickaxe.SetPickaxe(level);
            SetPickaxe(newPickaxe);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { }

    public void UpdatePickaxeState()
    {
        if (transform.childCount > 0)
        {
            _currentPickaxe = GetComponentInChildren<Pickaxe>();
        }
        else
        {
            _currentPickaxe = null;
        }
    }
}
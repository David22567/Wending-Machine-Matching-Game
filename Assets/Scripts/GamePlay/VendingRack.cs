using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingRack : MonoBehaviour
{
    public static VendingRack Instance;

    [SerializeField] private Transform[] rackSlots;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float selectedScale = 0.6f;
    [SerializeField] private float mergeDuration = 0.25f;

    private readonly List<Item> rackItems = new List<Item>();
    private readonly Stack<UndoSnapshot> undoStack = new Stack<UndoSnapshot>();
    private bool undoBlockedAfterCombo;

    private void Awake() => Instance = this;
    private void Update() => RotateRackItems();

    public bool IsRackFull() => rackItems.Count >= rackSlots.Length;
    public void ResetUndoBlock() => undoBlockedAfterCombo = false;

    public void AddItem(Item item, UndoSnapshot snapshot)
    {
        if (!undoBlockedAfterCombo) undoStack.Push(snapshot);

        int insertIndex = GetInsertIndex(item.itemType);
        rackItems.Insert(insertIndex, item);

        item.isInRack = true;
        item.GetComponent<Collider>().enabled = false;
        item.transform.localScale = item.originalScale * selectedScale;

        UpdateRackPositions();
        CheckForTriple(item.itemType);

        if (IsRackFull()) GameStateManager.Instance.Lose();
    }

    public void UndoLastMove()
    {
        if (undoBlockedAfterCombo || undoStack.Count == 0) return;

        UndoSnapshot snapshot = undoStack.Pop();

        if (snapshot.tappedItem != null && rackItems.Contains(snapshot.tappedItem))
        {
            rackItems.Remove(snapshot.tappedItem);
            snapshot.tappedItem.isInRack = false;
            snapshot.tappedItem.GetComponent<Collider>().enabled = true;
            snapshot.tappedItem.transform.localScale = snapshot.tappedItem.originalScale;
            snapshot.tappedItem.transform.rotation = Quaternion.identity;
            snapshot.tappedItem.MoveTo(snapshot.tappedOriginalPosition);
        }

        for (int i = 0; i < snapshot.movedItems.Count; i++)
        {
            if (snapshot.movedItems[i] != null)
                snapshot.movedItems[i].MoveTo(snapshot.movedOriginalPositions[i]);
        }

        UpdateRackPositions();
    }

    private int GetInsertIndex(ItemType type)
    {
        for (int i = 0; i < rackItems.Count; i++)
        {
            if (rackItems[i].itemType == type)
            {
                int j = i;
                while (j < rackItems.Count && rackItems[j].itemType == type) j++;
                return j;
            }
        }
        return rackItems.Count;
    }

    private void UpdateRackPositions()
    {
        for (int i = 0; i < rackItems.Count; i++)
            rackItems[i].MoveTo(rackSlots[i].position);
    }

    private void RotateRackItems()
    {
        foreach (Item item in rackItems)
        {
            if (item != null && item.isInRack)
                item.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }

    private void CheckForTriple(ItemType type)
    {
        int count = 0;
        foreach (Item item in rackItems) if (item.itemType == type) count++;
        if (count >= 3) StartCoroutine(MergeTriple(type));
    }

    private IEnumerator MergeTriple(ItemType type)
    {
        GameStateManager.Instance.RegisterCombo();
        undoBlockedAfterCombo = true;
        undoStack.Clear();

        List<Item> mergeItems = new List<Item>();
        for (int i = rackItems.Count - 1; i >= 0 && mergeItems.Count < 3; i--)
        {
            if (rackItems[i].itemType == type)
            {
                mergeItems.Add(rackItems[i]);
                rackItems.RemoveAt(i);
            }
        }

        mergeItems.Reverse();

        Item left = mergeItems[0];
        Item middle = mergeItems[1];
        Item right = mergeItems[2];
        Vector3 target = middle.transform.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / mergeDuration;
            left.transform.position = Vector3.Lerp(left.transform.position, target, t);
            right.transform.position = Vector3.Lerp(right.transform.position, target, t);
            yield return null;
        }

        Destroy(left.gameObject);
        Destroy(middle.gameObject);
        Destroy(right.gameObject);

        UpdateRackPositions();
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        foreach (Item item in FindObjectsOfType<Item>())
            if (!item.isInRack) return;

        GameStateManager.Instance.Win();
    }
}

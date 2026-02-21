using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Item : MonoBehaviour
{
    public ItemType itemType;
    public Vector3 originalScale;
    [HideInInspector] public bool isInRack;

    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private float columnTolerance = 0.05f;

    private Coroutine moveRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnTapped()
    {
        VendingRack.Instance.ResetUndoBlock();

        if (isInRack || VendingRack.Instance.IsRackFull())
            return;

        UndoSnapshot snapshot = new UndoSnapshot
        {
            tappedItem = this,
            tappedOriginalPosition = transform.position
        };

        ShiftDepthColumn(transform.position, snapshot);
        VendingRack.Instance.AddItem(this, snapshot);
    }

    void ShiftDepthColumn(Vector3 vacatedPos, UndoSnapshot snapshot)
    {
        Item[] allItems = FindObjectsOfType<Item>();
        List<Item> columnItems = new List<Item>();

        foreach (Item item in allItems)
        {
            if (item == this || item.isInRack)
                continue;

            Vector3 pos = item.transform.position;

            if (Mathf.Abs(pos.x - vacatedPos.x) > columnTolerance) continue;
            if (Mathf.Abs(pos.y - vacatedPos.y) > columnTolerance) continue;
            if (pos.z <= vacatedPos.z) continue;

            columnItems.Add(item);
        }

        columnItems.Sort((a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        Vector3 targetPos = vacatedPos;

        foreach (Item item in columnItems)
        {
            snapshot.movedItems.Add(item);
            snapshot.movedOriginalPositions.Add(item.transform.position);

            Vector3 nextTarget = item.transform.position;
            item.MoveTo(targetPos);
            targetPos = nextTarget;
        }
    }

    public void MoveTo(Vector3 targetPos)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveRoutine(targetPos));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        Vector3 start = transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            transform.position = Vector3.Lerp(start, target, eased);
            yield return null;
        }

        transform.position = target;
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UndoSnapshot
{
    public Item tappedItem;
    public Vector3 tappedOriginalPosition;
    public List<Item> movedItems = new List<Item>();
    public List<Vector3> movedOriginalPositions = new List<Vector3>();
}

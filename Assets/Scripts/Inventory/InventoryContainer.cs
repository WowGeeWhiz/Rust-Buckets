using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContainer : MonoBehaviour
{
    private InventoryNode firstItem;
    public ref InventoryNode FirstItem()
    {
        return ref firstItem;
    }
    public void SetFirstItem(ref InventoryNode input)
    {
        firstItem = input;
    }

    private InventoryNode equippedFirstItem;
    public ref InventoryNode EquippedFirstItem()
    {
        return ref equippedFirstItem;
    }
    public void SetEquippedFirstItem(ref InventoryNode input)
    {
        equippedFirstItem = input;
    }

    public ref InventoryNode GetItemAtPos(int pos)
    {
        InventoryNode output = firstItem;
        for (int i = 0; i < pos; i++)
        {
            
        }
    }

    private ref InventoryNode GetItemRecursive (InventoryNode currentNode)
    {

    }
}

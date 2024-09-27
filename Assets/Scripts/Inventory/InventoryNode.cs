using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryNode : InventoryContainer
{
    public Item item; 

    private InventoryNode next;
    public ref InventoryNode Next()
    {
        return ref next;
    }
    public void SetNext(ref InventoryNode newNext)
    {
        next = newNext;
    }

    /*may not be necessary */
    private int listPosition;
    public int ListPosition => listPosition;
    public void SetListPosition(int newPosition)
    {
        listPosition = newPosition;
    }
    //*/


    public InventoryNode(ref Item inputItem, int inputPosition)
    {
        item = inputItem;
        listPosition = inputPosition;
    }

    public List<Item> GetItemsRecursiveStart()
    {
        List<Item> list = new List<Item>();

        list = GetItemsRecursive(list);
        return list;
    }


    public List<Item> GetItemsRecursive(List<Item> list)
    {
        list.Add(item);
        if (next != null)
        {
            return next.GetItemsRecursive(list);
        }
        return list;
    }
}

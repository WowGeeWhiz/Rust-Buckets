using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryNode : InventoryContainer
{
    public GameObject item; //replace with actual item script, dont rename

    private InventoryNode next;
    public ref InventoryNode Next()
    {
        return ref next;
    }
    public void SetNext(ref InventoryNode newNext)
    {
        next = newNext;
    }

    private InventoryNode nextEquipped;
    public ref InventoryNode NextEquipped()
    {

        return ref nextEquipped;
    }
    public void SetNextEquipped(InventoryNode newEquipped)
    {
        nextEquipped = newEquipped;
    }

    /*may not be necessary */
    private int listPosition;
    public int ListPosition => listPosition;
    public void SetListPosition(int newPosition)
    {
        listPosition = newPosition;
    }
    //*/


    public InventoryNode(ref GameObject inputItem, int inputPosition)
    {
        item = inputItem;
        listPosition = inputPosition;
    }
}

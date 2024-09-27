using System;
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

    public ref InventoryNode GetNodeAtPos(int pos)
    {
        ref InventoryNode output = ref firstItem;
        for (int i = 0; i < pos; i++)
        {
            if (output.Next() != null)
            {
                output = ref output.Next();
            }
            else
            {
                Debug.LogError($"GetItemAtPos({pos}) index limit reached");
                break;
            }
        }

        return ref output;
    }


    public CombatInventory combatInventory;
    public void GenerateCombatInventory()
    {
        combatInventory.GenerateCombatInventory(firstItem.GetItemsRecursiveStart());
    }

    //function to get all of a type in the inventory
    //function to get all equipped in the inventory
    //function to get all equipped of a type in the inventory

    //need to display inventory items
    //allow selection of an item for each tpye slot
    //allow adding items to character inventory
    //inventory ui setup

}

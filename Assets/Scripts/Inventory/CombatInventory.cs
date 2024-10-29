using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CombatInventory : MonoBehaviour
{
    public CombatNode currentWeapon;

    //populate this inventory from the equipped items in the main inventory
    public void GenerateCombatInventory(List<Item> items)
    {
        Item item1 = null, item2 = null, item3 = null;
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsEquipped)
            {
                if (item1 == null)
                {
                    item1 = items[i];
                }
                else if (item2 == null)
                {
                    item2 = items[i];
                }
                else if (item3 == null)
                {
                    item3 = items[i];
                }
                else
                {
                    Debug.LogError("More than 3 equipped items");
                    break;
                }
            }
        }
        currentWeapon = new CombatNode(item1);
        currentWeapon.next = new CombatNode(item2, currentWeapon);
        currentWeapon.prev = new CombatNode(item3, currentWeapon.next, currentWeapon);
        currentWeapon.next.next = currentWeapon.prev;
    }

    //for use in a scene when running over an item on the ground
    public void Pickup(Item item)
    {
        if (currentWeapon.item == null)
        {
            currentWeapon.item = item;
        }
        else if (currentWeapon.next.item == null)
        {
            currentWeapon.next.item = item;
        }
        else if (currentWeapon.prev.item == null)
        {
            currentWeapon.prev.item = item;
        }
        else
        {
            currentWeapon.item = item;
            //currentWeapon.item.Drop(); //insert method here to drop currently equipped item in the worldspace
        }
    }

    public void NextItem()
    {
        currentWeapon = currentWeapon.next;
    }

    public void PrevItem()
    {
        currentWeapon = currentWeapon.prev;
    }
}

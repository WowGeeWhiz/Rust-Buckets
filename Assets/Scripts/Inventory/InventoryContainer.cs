using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.Unicode;
using UnityEngine.SceneManagement;
using Fusion.Addons.SimpleKCC;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using Unity.Netcode.Transports.UTP;
public class InventoryContainer : MonoBehaviour
{
    public InventoryNode firstItem;
    public string inventoryLevelName = "mainMenu";
    public bool debugInventoryContainer = false;

    public void Start()
    {
        if (debugInventoryContainer) Debug.Log("Start called");
        if (SceneManager.GetActiveScene().name != "mainMenu")
        {
            if (debugInventoryContainer)
            {
                Item temp = (Item) ScriptableObject.CreateInstance("Item");
                firstItem = new InventoryNode(ref temp, 0);
            }
            GenerateCombatInventory();
        }
    }

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


    //combat inventory is the inventory used in a match that just has the equipped weapons
    public CombatInventory combatInventory;
    //needs to trigger when loading into a level
    public void GenerateCombatInventory()
    {
        if (debugInventoryContainer) Debug.Log("Generating combat inventory");
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

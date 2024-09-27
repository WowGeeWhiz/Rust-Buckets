using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatNode : MonoBehaviour
{
    public Item item;

    public CombatNode next;
    public CombatNode prev;

    public CombatNode(Item item_)
    {
        item = item_;
        prev = null;
        next = null;
    }
    
    public CombatNode(Item item_, CombatNode prev_)
    {
        item = item_;
        prev = prev_;
        next = null;
    }
    public CombatNode(Item item_, CombatNode prev_, CombatNode next_)
    {
        item = item_;
        prev = prev_;
        next = next_;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{

    private bool isEquipped;
    public bool IsEquipped => isEquipped;
    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

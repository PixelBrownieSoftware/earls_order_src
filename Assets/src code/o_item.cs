using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class o_item : s_object {

    public enum IT_TYPE
    {
        HEALTH,
        AP
    }
    public IT_TYPE ITEM_TYPE;
    public int amount;

	void Start () {
        walkable = true;	
	}
}

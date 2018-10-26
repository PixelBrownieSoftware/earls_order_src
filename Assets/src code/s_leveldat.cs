using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_leveldat
{
    public s_leveldat(s_object[,] characters, s_object[,] items, s_object[,] blocks, Vector2Int gridsize)
    {
        nodes_character.Clear();
        nodes_blocks.Clear();
        nodes_items.Clear();
        this.gridsize = gridsize;

        for (int x = 0; x < gridsize.x; x++)
        {
            for (int y = 0; y < gridsize.y; y++)
            {

                if (characters[x, y] != null)
                {
                    nodes_character.Add(new s_nodedat(x, y, characters[x, y].name));
                }

                if (blocks[x, y] != null)
                {
                    SpriteRenderer sprred = blocks[x, y].GetComponent<SpriteRenderer>();
                    Sprite spr = sprred.sprite;
                    nodes_blocks.Add(new s_nodedat(x, y, blocks[x, y].name, spr, sprred.gameObject.transform.localRotation));
                }

                if (items[x, y] != null)
                {
                    nodes_items.Add(new s_nodedat(x, y, items[x, y].name));
                }
            }
        }
    }
    public Vector2Int gridsize;
    public List<s_nodedat> nodes_character = new List<s_nodedat>();
    public List<s_nodedat> nodes_items = new List<s_nodedat>();
    public List<s_nodedat> nodes_blocks = new List<s_nodedat>();
}
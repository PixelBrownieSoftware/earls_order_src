using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class s_leveledit : MonoBehaviour {
    s_grid Grid;
    public int select = 0;
    public s_object selected_object;
    int x = 0;
    int y = 0;
    float angle = 0;

    public Sprite[] spriteArray;
    public int spritenum = 0;
    public SpriteRenderer examplerend;

    private void Start()
    {
        Grid = GameObject.Find("Manager").GetComponent<s_grid>();
    }

    void UpdateToolText()
    {
        Text canvtext = GameObject.Find("Canvas Text").GetComponent<Text>();

        if (selected_object != null)
            canvtext.text = "Block selected: " + selected_object.name;
    }

    private void OnGUI()
    {
        x = (int)GUI.HorizontalSlider(new Rect(10, 40,180,15),x, 0, Grid.gridworldsize.x);
        GUI.Label(new Rect(10, 50, 180, 90), "X " + x);
        y = (int)GUI.HorizontalSlider(new Rect(10, 60 * 2, 180,15), y, 0, Grid.gridworldsize.y);
        GUI.Label(new Rect(10, 70 * 2 - 10, 180, 90), "Y " + y);

        if (GUILayout.Button("NewGrid"))
        {
            Grid.groundworldsize.y = y;
            Grid.groundworldsize.x = x;
            Grid.Initialize();
        }
    }

    public void SetObject(int menu)
    {
        List<s_object> blocks = Grid.blockObjects;
        select += menu;
        select = Mathf.Clamp(select, 0, blocks.Count - 1);

        selected_object = blocks[select];
        UpdateToolText();
    }

    private void Update()
    {
        if (Grid.gridworldsize.y > 0 && Grid.gridworldsize.x > 0)
        {
            Vector3 mousePositon = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            s_object selectedObj = Grid.ObjectFromWorld(mousePositon);

            if (Input.GetKeyDown(KeyCode.W))
                spritenum++;
            if (Input.GetKeyDown(KeyCode.S))
                spritenum--;


            if (Input.GetKeyDown(KeyCode.E))
                angle += 90;

            if (angle > 360)
                angle = 0;

            examplerend.gameObject.transform.localRotation = Quaternion.Euler(0, 0, angle);

            spritenum = Mathf.Clamp(spritenum, 0, spriteArray.Length - 1);

            examplerend.sprite = spriteArray[spritenum];

            if (Grid.NodeFromWorld(mousePositon) != null)
            {
                Vector2 snap = Grid.SnapToGrid(mousePositon);
                if (Input.GetMouseButton(0))
                {
                    if (selectedObj != null)
                    {
                        if (selectedObj.GetComponent<s_object>().GetType() == typeof(o_tile))
                        {
                            SpriteRenderer ren = selectedObj.GetComponent<SpriteRenderer>();
                            ren.sprite = spriteArray[spritenum];
                            ren.gameObject.transform.localRotation = examplerend.gameObject.transform.localRotation;
                        }
                    }
                    //target.transform.position = mousePositon;
                    if (Grid.ObjectFromWorld(mousePositon) == null)
                    {
                        Vector2Int vecint = Grid.VectorPositionFromWorld(mousePositon);

                        if (selected_object != null)
                        {
                            s_object obj = Grid.SpawnObject(selected_object.name, snap);
                        }
                    }

                }
                if (Input.GetMouseButton(1))
                {
                    if (Grid.ObjectFromWorld(mousePositon) != null)
                    {
                        Vector2Int vecint = Grid.VectorPositionFromWorld(mousePositon);
                        if (selected_object != null)
                        {
                            Grid.DespawnObject(snap);
                        }
                    }
                }
            }
        }
        
        if (Grid.nodes != null)
        {
            for (int x = 0; x < Grid.gridworldsize.x; x++)
                for (int y = 0; y < Grid.gridworldsize.y; y++)
                {
                    /*
                    SpriteRenderer colourRender = Grid.nodegameobjects[x, y].GetComponent<SpriteRenderer>();
                    colourRender.color = Color.white;
                    if (Grid.CheckForNodeOnGrid(mousePositon, x, y))
                    {
                        colourRender.color = Color.blue;
                    }
                    */
                }
        }


    }
    
    
    /*
                    if (nodes != null)
                    {
                        for (int x = 0; x < Grid.gridworldsize.x; x++)
                        {
                            for (int y = 0; y < Grid.gridworldsize.y; y++)
                            {
                                SpriteRenderer colourRender = Grid.nodegameobjects[x, y].GetComponent<SpriteRenderer>();
                                colourRender.color = Color.white;
                                if (Grid.CheckForNodeOnGrid(mousePositon, x, y))
                                {
                                    colourRender.color = Color.blue;
                                }
                                if (CheckForNodeOnGrid(target.transform.position, x, y))
                                {
                                    colourRender.color = Color.red;
                                }
                                if (CheckForNodeOnGrid(new Vector3(x * nodesize, y * nodesize), x, y))
                                {
                                    if (!NodeFromWorld(new Vector3(x * nodesize, y * nodesize)).walkable)
                                        colourRender.color = Color.magenta;
                                }

                                foreach (o_node n in opennodes)
                                {
                                    if (CheckForNodeOnGrid(n.position, x, y))
                                    {
                                        colourRender.color = Color.cyan;
                                    }
                                }
                                foreach (o_node n in closednodes)
                                {
                                    if (CheckForNodeOnGrid(n.position, x, y))
                                    {
                                        colourRender.color = Color.grey;
                                    }
                                }
                                foreach (o_node n in fringe)
                                {
                                    if (CheckForNodeOnGrid(n.position, x, y))
                                    {
                                        colourRender.color = Color.yellow;
                                    }
                                }
                                if (Grid.CheckForNodeOnGrid(goal, x, y))
                                {
                                    colourRender.color = Color.green;
                                }
                }

                        }
                    }*/
}
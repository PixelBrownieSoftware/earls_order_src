using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class s_grid : MonoBehaviour
{
    public GameObject goal, target;
    public GameObject nodbase;

    public o_node[,] nodes;
    public o_item[,] item_layer;
    public s_object[,] block_layer;
    public o_character[,] character_layer;
    public SpriteRenderer[,] GroundSprites;

    public Vector2Int gridworldsize;
    public Vector2Int groundworldsize;

    public int gridsizx, gridsizy;
    public int nodesize = 20;
    Dictionary<string, Queue<s_object>> objPool = new Dictionary<string, Queue<s_object>>();
    public List<s_object> blockObjects = new List<s_object>();

    private void Awake()
    {
        Initialize();
    }

    /*
    private void OnLevelWasLoaded(int level)
    {
        if (SceneManager.GetActiveScene().name.ToString() == "InGame")
            Initialize();
    }*/

    public void Initialize()
    {
        ClearGrid();
        objPool.Clear();
        character_layer = null;
        block_layer = null;
        item_layer = null;
        nodes = null;

        character_layer = new o_character[gridworldsize.x, gridworldsize.y];
        block_layer = new o_tile[gridworldsize.x, gridworldsize.y];
        item_layer = new o_item[gridworldsize.x, gridworldsize.y];
        nodes = new o_node[gridworldsize.x, gridworldsize.y];

        //if (nodes == null)

        if (GroundSprites == null)
        {
            GenerateGround();
        }

        for (int x = 0; x < gridworldsize.x; x++)
            for (int y = 0; y < gridworldsize.y; y++)
            {
                nodes[x, y] = new o_node();
                nodes[x, y].position = new Vector2(nodesize * x, nodesize * y);
                
            }
        for (int x = 0; x < groundworldsize.x; x++)
            for (int y = 0; y < groundworldsize.y; y++)
            {
                GroundSprites[x, y].color = Color.white;
            }

        CreateObjectPooler();
        FillObjectPooler();
        
        print("Inititalized level!");

        if (SceneManager.GetActiveScene().name.ToString() == "InGame")
            s_levelloader.load.LoadData();
        

        //s_charactermanager.SetGameMode();
        
    }

    void GenerateGround() {

        GroundSprites = new SpriteRenderer[gridworldsize.x, gridworldsize.y];
        for (int x = 0; x < gridworldsize.x; x++)
            for (int y = 0; y < gridworldsize.y; y++)
            {
                GroundSprites[x, y] = (Instantiate(nodbase, new Vector2(x * nodesize, y * nodesize), Quaternion.identity).GetComponent<SpriteRenderer>());
                GroundSprites[x, y].color = Color.clear;
            }
    }

    void FillObjectPooler()
    {
        foreach (s_object obj in blockObjects)
        {
            Type type = obj.GetType();

            if (type == typeof(o_tile))
            {
                for (int i = 0; i < 900; i++)
                {
                    s_object objectt = Instantiate(obj, transform.position, Quaternion.identity);
                    objectt.name = obj.name;
                    objPool[obj.name].Enqueue(objectt);
                    objectt.gameObject.SetActive(false);
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    s_object objectt = Instantiate(obj, transform.position, Quaternion.identity);
                    objectt.name = obj.name;
                    objPool[obj.name].Enqueue(objectt);
                    objectt.gameObject.SetActive(false);
                }
            }
        }
    }

    void CreateObjectPooler()
    {
        foreach (s_object obj in blockObjects)
        {
            if (objPool.ContainsKey(obj.name))
                continue;

            Queue<s_object> objects = new Queue<s_object>();
            Type type = obj.GetType();

            objPool.Add(obj.name, objects);
        }
    }

    public void PaintNode(Vector2Int position, Color colour)
    {
        GroundSprites[position.x, position.y].color = colour;
    }

    public void UnpaintAllNodes()
    {
        for (int x = 0; x < groundworldsize.x; x++)
            for (int y = 0; y < groundworldsize.y; y++)
            {
                GroundSprites[x, y].color = Color.white;

            }
    }

    private void Update()
    {
        if (nodes != null)
        {
            CalculateNodes();
        }
        if (GroundSprites != null)
        {
            /*
            for (int x = 0; x < gridworldsize.x; x++)
            {
                for (int y = 0; y < gridworldsize.y; y++)
                {
                    if (block_layer[x,y] != null)
                    {
                        PaintNode(new Vector2Int(x, y), Color.magenta);
                    }
                    
                }

            }
            */
        }
    }

    public void CalculateNodes()
    {
        
        /*
        foreach (o_character obj in character_layer)
        {
            print(obj.name);
            Vector2Int vec = NodePositionFromWorld(NodeFromWorld(obj.transform.position));
            nodes[vec.x, vec.y].walkable = false;
        }
        */

        for (int x = 0; x < gridworldsize.x; x++)
            for (int y = 0; y < gridworldsize.y; y++)
            {
                if (character_layer[x, y] != null || block_layer[x, y] != null)
                    nodes[x, y].walkable = false;
                else if(character_layer[x, y] == null && block_layer[x, y] == null)
                    nodes[x, y].walkable = true;

            }
        ResetNodes();
    }
    
    public HashSet<o_node> CheckAroundNode(o_node node)
    {
        HashSet<o_node> nodelist = new HashSet<o_node>();
        Vector2Int nodepos = NodePositionFromWorld(node);

        int xno = nodepos.x, yno = nodepos.y;
        for (int x = xno + 1; x >= xno - 1; x--)
        {
            if (x == xno)
                continue;

            if (x > 0 && x < gridworldsize.x)
            {
                nodelist.Add(nodes[x, yno]);
            }

            for (int y = yno + 1; y >= yno - 1; y--)
            {
                if (y == yno)
                    continue;

                if (y > 0 && y < gridworldsize.y)
                {
                    /*
                    if (yno - y == 1)
                        nodes[xno, y].DIR = o_node.DIRECTION.UP;

                    if (yno - y == -1)
                        nodes[xno, y].DIR = o_node.DIRECTION.DOWN;
                    */
                    nodelist.Add(nodes[xno, y]);
                }

            }
        }
        return nodelist;
    }
    public List<o_node> CheckAroundNode(o_node node, s_pathfind path)
    {
        List<o_node> nodelist = new List<o_node>();
        Vector2Int nodepos = NodePositionFromWorld(node);

        int xno = nodepos.x, yno = nodepos.y;
        for (int x = xno + 1; x >= xno - 1; x--)
        {
            if (x == xno)
                continue;

            if (x > 0 && x < gridworldsize.x)
            {

                nodelist.Add(nodes[x, yno]);
            }

            for (int y = yno + 1; y >= yno - 1; y--)
            {
                if (y == yno)
                    continue;

                if (y > 0 && y < gridworldsize.y)
                {
                    

                    nodelist.Add(nodes[xno, y]);
                }

            }
        }
        return nodelist;
    }

    public void ClearGrid()
    {
        if (character_layer != null)
        {
            foreach (o_character c in character_layer)
                if (c != null)
                    DespawnObject(c);
        }

        if (block_layer != null)
        {
            foreach (s_object b in block_layer)
                DespawnObject(b);
        }

        if (item_layer != null)
        {
            foreach (o_item i in item_layer)
                DespawnObject(i);
        }

        if(GroundSprites != null)
            foreach (SpriteRenderer re in GroundSprites)
            {
                re.color = Color.clear;
            }
        
        character_layer = null;
        block_layer = null;
        item_layer = null;

        character_layer = new o_character[gridworldsize.x, gridworldsize.y];
        block_layer = new o_tile[gridworldsize.x, gridworldsize.y];
        item_layer = new o_item[gridworldsize.x, gridworldsize.y];

    }

    public void ResetNodes()
    {
        for (int x = 0; x < gridworldsize.x; x++)
            for (int y = 0; y < gridworldsize.y; y++)
            {
                nodes[x, y].g = 0;
                nodes[x, y].h = Mathf.Infinity;
            }

    }

    public s_object SpawnObject(string typeofobj, Vector2 positon)
    {
        if (objPool[typeofobj] == null)
        {
            print(typeofobj);
            return null;
        }
        s_object obj = objPool[typeofobj].Peek();
        //if(objPool[typeofobj].Peek())

        Vector2Int cells = VectorPositionFromWorld(positon);
        /*
        bool check = false;

        //Compare the 'obj' to what exists on the current position
        if (obj.GetType() == typeof(o_character))
            check = character_layer[cells.x, cells.y] != null;
        else if (obj.GetType() == typeof(o_tile))
            check = block_layer[cells.x, cells.y] != null;
        else if (obj.GetType() == typeof(o_item))
            check = item_layer[cells.x, cells.y] != null;
        
        if (check)
            return null;
        */

        //Delete the last position from the world
        Vector2Int lastpos = VectorPositionFromWorld(obj.transform.position);
        /*
        if (obj.GetType() == typeof(o_character))
            character_layer[lastpos.x, lastpos.y] = null;
        else if(obj.GetType() == typeof(o_tile))
            block_layer[lastpos.x, lastpos.y] = null;
        else if (obj.GetType() == typeof(o_item))
            item_layer[lastpos.x, lastpos.y] = null;
        */

        if (obj.GetType() == typeof(o_character))
            character_layer[cells.x, cells.y] = (o_character)obj;
        else if (obj.GetType() == typeof(o_tile))
            block_layer[cells.x, cells.y] = (o_tile)obj;
        else if (obj.GetType() == typeof(o_item))
            item_layer[cells.x, cells.y] = (o_item)obj;

        objPool[typeofobj].Dequeue();
        objPool[typeofobj].Enqueue(obj);
        obj.transform.position = SnapToGrid(positon);
        obj.gameObject.SetActive(true);

        return obj;
    }

    public void DespawnObject(s_object obje)
    {
        s_object obj = obje;
        
        if (obj == null)
            return;

        Vector2Int lastpos = VectorPositionFromWorld(obj.transform.position);
        if (obj.GetType() == typeof(o_character))
            character_layer[lastpos.x, lastpos.y] = null;
        else if(obj.GetType() == typeof(o_tile))
            block_layer[lastpos.x, lastpos.y] = null;
        else if (obj.GetType() == typeof(o_item))
            item_layer[lastpos.x, lastpos.y] = null;

        obj.gameObject.SetActive(false);
    }
    public void DespawnObject(s_object obje, int layerInt)
    {
        s_object obj = obje;
        /*
        if (obj == null)
            return;
        */

        Vector2Int lastpos = VectorPositionFromWorld(obj.transform.position);
        if (layerInt == 0)
            character_layer[lastpos.x, lastpos.y] = null;
        else if (layerInt == 1)
            block_layer[lastpos.x, lastpos.y] = null;
        else if (layerInt == 2)
            item_layer[lastpos.x, lastpos.y] = null;

        obj.gameObject.SetActive(false);
    }
    public void DespawnObject(Vector2 positon)
    {
        s_object obj = ObjectFromWorld(positon);

        if (obj == null)
            return;
        Vector2Int lastpos = VectorPositionFromWorld(obj.transform.position);
        if (obj.GetType() == typeof(o_character))
            character_layer[lastpos.x, lastpos.y] = null;
        else if (obj.GetType() == typeof(o_tile))
            block_layer[lastpos.x, lastpos.y] = null;
        else if (obj.GetType() == typeof(o_item))
            item_layer[lastpos.x, lastpos.y] = null;

        obj.gameObject.SetActive(false);
    }

    public Vector2 SnapToGrid(Vector2 vec)
    {
        Vector2Int intv = VectorPositionFromWorld(vec);
        return new Vector2(intv.x * nodesize, intv.y * nodesize);
    }
    
    public o_node NodeFromWorld(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / nodesize);
        int y = Mathf.RoundToInt(position.y / nodesize);

        if (x > gridworldsize.x - 1 || x < 0 || y < 0 || y > gridworldsize.y - 1)
            return null;
        if (nodes[x, y] == null)
            return null;

        return nodes[x, y];
    }

    public bool CheckForNodeOnGrid(Vector3 node, int x, int y)
    {
        if (NodeFromWorld(node) == null)
            return false;

        if (x * 20 == NodeFromWorld(node).position.x
            && y * 20 == NodeFromWorld(node).position.y)
            return true;

        return false;
    }
    public bool CheckForNodeOnGrid(GameObject node, int x, int y)
    {
        if (ObjectFromWorld(node.transform.position) == null)
            return false;

        if (x * 20 == NodeFromWorld(node.transform.position).position.x
            && y * 20 == NodeFromWorld(node.transform.position).position.y)
            return true;

        return false;
    }

    public s_object ObjectFromWorld(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / nodesize);
        int y = Mathf.RoundToInt(position.y / nodesize);

        if((x > gridworldsize.x - 1 || x < 0
            || y < 0 || y > gridworldsize.y - 1))
            return null;

        if (item_layer == null || 
            character_layer == null ||
            block_layer == null)
            return null;

        if (item_layer[x, y] == null 
            && character_layer[x, y] == null 
            && block_layer[x, y] == null)
            return null;
        
        if (character_layer[x, y] != null)
            return character_layer[x, y];

        if (item_layer[x, y] != null)
            return item_layer[x, y];

        if (block_layer[x, y] != null)
            return block_layer[x, y];

        return null;
    }
    public s_object ObjectFromWorld(Vector3 position, int layer)
    {
        //TODO: REFRACTOR THIS PART OF THE CODE TO MAKE IT SO THAT THE LAYERS ARE CHECKED
        //I.E. PLAYER OR ITEM

        int x = Mathf.RoundToInt(position.x / nodesize);
        int y = Mathf.RoundToInt(position.y / nodesize);

        if (item_layer[x, y] == null
            && character_layer[x, y] == null
            && block_layer[x, y] == null
            || (x > gridworldsize.x - 1 || x < 0
            || y < 0 || y > gridworldsize.y - 1))
            return null;

        if (layer == 0)
        {
            if (character_layer[x, y] != null)
                return character_layer[x, y];
        }
        else if (layer == 1)
        {
            if (item_layer[x, y] != null)
                return item_layer[x, y];
        }
        
        if (block_layer[x, y] != null)
            return block_layer[x, y];
        
        return null;
    }
    public s_object ObjectFromWorld(o_node node)
    {
        int x = Mathf.RoundToInt(node.position.x / nodesize);
        int y = Mathf.RoundToInt(node.position.y / nodesize);

        if (item_layer[x, y] == null
            && character_layer[x, y] == null
            && block_layer[x, y] == null
            || (x > gridworldsize.x - 1 || x < 0
            || y < 0 || y > gridworldsize.y - 1))
            return null;

        if (character_layer[x, y] != null)
            return character_layer[x, y];

        if (item_layer[x, y] != null)
            return item_layer[x, y];

        if (block_layer[x, y] != null)
            return block_layer[x, y];

        return null;
    }

    public Vector2Int NodePositionFromWorld(o_node node)
    {
        int x = Mathf.RoundToInt(node.position.x / nodesize);
        int y = Mathf.RoundToInt(node.position.y / nodesize);
        return new Vector2Int(x, y);
    }

    public Vector2Int VectorPositionFromWorld(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / nodesize);
        int y = Mathf.RoundToInt(pos.y / nodesize);
        return new Vector2Int(x, y);
    }

    int[,] NodeDivision(o_node nodetarg)
    {
        return new int[((int)nodetarg.position.x + 20 / 2) / 20, ((int)nodetarg.position.y + 20 / 2) / 20];
    }

}

[System.Serializable]
public class o_node
{
    public Vector2 position;
    public bool walkable = true;
    public o_node parent;
    

    public float g, h;
    public float f
    {
        get {
            return h + g;
        }
    }

}
/*
   private void Update()
   {
       fringe.Clear();
       pathfind();
       Retrace(getnode(target), getnode(goal));
   }

   private void Awake()
   {
       gridsizx = (int)gridworldsize.x;
       gridsizy = (int)gridworldsize.y;

       nodes = new o_node[gridsizx, gridsizy];
       for (int x = 0; x < gridsizx; x++)
       {
           for (int y = 0; y < gridsizy; y++)
           {
               nodes[x, y] = new o_node();
               nodes[x, y].g = Mathf.Infinity;
               nodes[x, y].h = Mathf.Infinity;
               nodes[x, y].position = new Vector2(nodesize * x, nodesize * y);
           }
       }
   }


   o_node getnode(GameObject node)
   {
       float xpos = (node.transform.position.x + gridworldsize.x / 2) / gridworldsize.x;
       float ypos = (node.transform.position.y + gridworldsize.y / 2) / gridworldsize.y;
       ypos = Mathf.Clamp01(ypos);
       xpos = Mathf.Clamp01(xpos);


       int y = Mathf.RoundToInt((20 - 1) * ypos);
       int x = Mathf.RoundToInt((20 - 1) * xpos);

       return nodes[x,y];
   }

   void Retrace(o_node start, o_node end)
   {
       List<o_node> path = new List<o_node>();
       o_node currentnode = end;


       while(currentnode != start)
       {
           path.Add(currentnode);
           currentnode = currentnode.parent;
       }
       path.Reverse();
       fringe = path;
   }


   Vector2Int GridToWorldPos(o_node node)
   {

       float xpos = (node.position.x + gridworldsize.x / 2) / gridworldsize.x;
       float ypos = (node.position.y + gridworldsize.y / 2) / gridworldsize.y;
       ypos = Mathf.Clamp01(ypos);
       xpos = Mathf.Clamp01(xpos);

       print(xpos);

       int y = Mathf.RoundToInt((20 - 1) * ypos);
       int x = Mathf.RoundToInt((20 - 1) * xpos);
       return new Vector2Int( x, y);
   }

   List<o_node> DirectionCheck(o_node nodetarg)
   {
       List<o_node> node = new List<o_node>();
       int nodx = GridToWorldPos(nodetarg).x, nody = GridToWorldPos(nodetarg).y;

       //print(nodx);

       for (int x = nodx + 1; x > nodx - 1; x--)
       {
           for (int y = nody + 1; y > nody - 1; y--)
           {
               print("x: " + x + " y: " + y);
               if (y == nody && x == nodx)
                   continue;


               int checkx = x ;
               int checky =  y ;

               if (checkx >= 0 && checkx < 20 || checky >= 0 && checky < 20)
               {
                   node.Add(nodes[x, y]);
               }
           }
       }
       return node;
   }


   void pathfind()
   {
       HashSet<o_node> closed = new HashSet<o_node>();
       List<o_node> open = new List<o_node>();

       open.Add(getnode(target));

       while (open.Count > 0)
       {
           o_node currentnode = open[0];
           for (int i = 1; i < open.Count; i++)
           {
               if (open[i].f < currentnode.f || open[i].f == currentnode.f && open[i].h < currentnode.h)
                   currentnode = open[i];
           }

           open.Remove(currentnode);
           closed.Add(currentnode);

           if (currentnode == getnode(goal))
               return;

           foreach (o_node n in DirectionCheck(currentnode))
           {
               if (!n.walkable || closed.Contains(n))
                   continue;

               int movcost = (int)currentnode.g + (int)HerusticVal(currentnode.position, n.position);
               //print((int)HerusticVal(currentnode.position, n.position));

               if (movcost < n.g || !open.Contains(n))
               {
                   n.g = movcost;
                   n.h = HerusticVal(n.position, goal.transform.position);
                   n.parent = currentnode;
                   fringe.Add(n);
                   if (!open.Contains(n))
                   {
                       open.Add(n);
                   }
               }

           }

       }


   }

   /*
   while (currentnode != getnodegoal())
       {
   */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_pathfind : MonoBehaviour {

    s_grid Grid;
    public List<o_node> fringe = new List<o_node>();

    public List<o_node> closednodes = new List<o_node>();
    List<o_node> opennodes = new List<o_node>();

    public void Start()
    {
        Grid = GetComponent<s_grid>();
    }

    void CleanDebugNodes()
    {
        fringe.Clear();
        closednodes.Clear();
        opennodes.Clear();
        Vector2 gridworld = Grid.gridworldsize;

        for (int x = 0; x < gridworld.x; x++)
        {
            for (int y = 0; y < gridworld.y; y++)
            {
                Grid.UnpaintAllNodes();

            }
        }
    }

    public bool PathFind(Vector2 target, Vector2 goal)
    {
        List<o_node> openlist = new List<o_node>();
        HashSet<o_node> closelist = new HashSet<o_node>();

        o_node startnode = Grid.NodeFromWorld(target);
        o_node goalnode = Grid.NodeFromWorld(goal);
        
        if (goalnode == null ||
            //!startnode.walkable ||
            !goalnode.walkable)
        {
            //print("You're not a bloody ghost!");
            return false;
        }

        startnode.h = HerusticVal(startnode.position, goal);
        openlist.Add(startnode);

        while (openlist.Count > 0)
        {
            o_node currentnode = openlist[0];
            //print(currentnode.h);
            for (int i = 1; i < openlist.Count; i++)
            {
                if (openlist[i].f <= currentnode.f && openlist[i].h < currentnode.h)
                    currentnode = openlist[i];
            }

            openlist.Remove(currentnode);
            closelist.Add(currentnode);
            //closednodes.Add(currentnode);   //For visual

            if (currentnode == goalnode)
            {
                Grid.ResetNodes();
                return true;
            }

            foreach (o_node neighbour in Grid.CheckAroundNode(currentnode, this))
            {
                float newcost = currentnode.g + HerusticVal(currentnode.position, neighbour.position);
                if (!neighbour.walkable || closelist.Contains(neighbour))
                    continue;

                if (newcost < neighbour.g || !openlist.Contains(neighbour))
                {
                    neighbour.g = newcost;
                    neighbour.h = HerusticVal(neighbour.position, goal);
                    neighbour.parent = currentnode;

                    if (!openlist.Contains(neighbour))
                    {
                        //opennodes.Add(neighbour);
                        openlist.Add(neighbour);
                    }
                }
            }
        }
        return false;
    }

    float HerusticVal(Vector2 a, Vector2 b)
    {
        float distx = Mathf.Abs(a.x - b.x);
        float disty = Mathf.Abs(a.y - b.y);

        return Vector2.Distance(a, b);

        //D * (distx + dist)
    }

    public List<o_node> RetracePath(o_node current, o_node start)
    {
        Grid.UnpaintAllNodes();
        List<o_node> path = new List<o_node>();

        o_node currentnode = current;
        
        while (currentnode != start)
        {
            path.Add(currentnode);
            {
                Vector2Int nodepos = Grid.VectorPositionFromWorld(currentnode.position);

                //Grid.PaintNode(nodepos, Color.white);
                HashSet<o_node> neigh = Grid.CheckAroundNode(current);

                //print(currentnode.DIR);
            }
            currentnode = currentnode.parent;
        }
        path.Reverse();

        /*
        print(Grid.ObjectFromWorld(path[0]));
        print(Grid.ObjectFromWorld(path[path.Count - 1]));
        
        if (Grid.ObjectFromWorld(path[path.Count - 1]) != null)
        {
            s_object obj = Grid.ObjectFromWorld(path[path.Count - 1]);
            while (path.Count != 0 &&
                obj != null && 
                obj.GetType() == typeof(o_character))
            {
                path.Remove(path[path.Count - 1]);
                if (path.Count > 0)
                    if (Grid.ObjectFromWorld(path[path.Count - 1]) != null)
                        obj = Grid.ObjectFromWorld(path[path.Count - 1]);
                else
                    break;
            }
        }*/
        //path.Add(start);
        return path;
    }
}

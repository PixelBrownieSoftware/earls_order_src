using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class o_character : s_object, ICharacter {

    public int health, maxHealth;
    public int attack;
    public int actionPoints, maxActionPoints;
    public bool playable = false;
    bool defeated = false;
    bool hasdisapeared = false;

    public o_item current_item = null;
    public int range;   //For AI

    public new SpriteRenderer renderer;
    public AudioClip walksound;

    s_grid grid;
    s_pathfind path;
    s_charactermanager charctMnger;

    public List<o_node> directions = new List<o_node>();

    void ICharacter.Intialize()
    {
        defeated = false;
        current_item = null;
        actionPoints = maxActionPoints;
        health = maxHealth;
        hasdisapeared = false;
        GameObject manager = GameObject.Find("Manager");
        if(GameObject.Find("CharacterManager") != null)
            charctMnger = GameObject.Find("CharacterManager").GetComponent<s_charactermanager>();

        renderer = GetComponent<SpriteRenderer>();
        grid = manager.GetComponent<s_grid>();
        path = manager.GetComponent<s_pathfind>();
    }

	void Start ()
    {
        walkable = false;
	}

    public bool IsDisappear()
    {
        return hasdisapeared;
    }

    public int MoveCost(Vector3 cursorpos)
    {
        Vector3 mousePositon = cursorpos;

        if (path.PathFind(transform.position, mousePositon))
        {
            o_node goal = grid.NodeFromWorld(mousePositon);
            o_node thischaracter = grid.NodeFromWorld(transform.position);

            if (!grid.NodeFromWorld(mousePositon).walkable)
            {
                s_object objectm = grid.ObjectFromWorld(mousePositon);

                return -1;
            }
            else return path.RetracePath(goal, thischaracter).Count;
        }
        else
            return -1;
    }

    public void SetDirections(o_node goal)
    {
        o_node thischaracter = grid.NodeFromWorld(transform.position);
        directions = path.RetracePath(goal, thischaracter);
    }

    public void Teleport(s_charactermanager returning_system)
    {
        positon = grid.VectorPositionFromWorld(transform.position);
        Queue<o_node> di = new Queue<o_node>();
        foreach (o_node n in directions)
        {
            di.Enqueue(n);
        }
        Vector2Int newpos = new Vector2Int();
        while (di.Count != 1)
        {
            di.Dequeue();
        }
        transform.position = di.Peek().position; 
        newpos = grid.VectorPositionFromWorld(transform.position);
        grid.character_layer[positon.x, positon.y] = null;
        grid.character_layer[newpos.x, newpos.y] = this;
        grid.UnpaintAllNodes();
        di.Clear();

        returning_system.LooseFocus();
    }

    public IEnumerator MoveToPosition(s_charactermanager returning_system)
    {
        positon = grid.VectorPositionFromWorld(transform.position);
        Queue<o_node> di = new Queue<o_node>();
        foreach (o_node n in directions)
        {
            di.Enqueue(n);
        }
        Vector2Int newpos = new Vector2Int();
        while (di.Count != 0)
        {
            Vector3 dir = (di.Peek().position - (Vector2)transform.position).normalized;

            s_sound.PlaySound(walksound);
            yield return new WaitForSeconds(0.05f);

            transform.Translate(dir * 20);
            di.Dequeue();
        }

        newpos = grid.VectorPositionFromWorld(transform.position);
        grid.character_layer[positon.x, positon.y] = null;
        grid.character_layer[newpos.x, newpos.y] = this;
        grid.UnpaintAllNodes();
        
            returning_system.LooseFocus();
    }

    public IEnumerator Defeat()
    {
        defeated = true;
        s_sound.PlaySound(sound);
        for (int i = 0; i < 5; i++)
        {
            renderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            renderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        if (playable)
            charctMnger.Players.Remove(this);
        else
            charctMnger.Enemies.Remove(this);

        if (current_item != null)
            if (grid.ObjectFromWorld(transform.position) != null)
                grid.SpawnObject(current_item.name, transform.position);

        grid.DespawnObject(this);
        hasdisapeared = true;
    }
    
    void Update()
    {
        if (health <= 0)
            if (!defeated)
                StartCoroutine(Defeat());
    }
}
public interface ICharacter
{
    void Intialize();

}

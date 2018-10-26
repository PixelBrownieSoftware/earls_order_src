using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;


[System.Serializable]
public struct s_nodedat
{
    public s_nodedat(int x, int y, string name)
    {
        this.x = x;
        this.y = y;
        objectstr = name;
        spr = null;
        rot = Quaternion.Euler(0, 0, 0);
    }
    public s_nodedat(int x, int y, string name, Sprite spr, Quaternion rot)
    {
        this.x = x;
        this.y = y;
        objectstr = name;
        this.spr = spr;
        this.rot = rot;
    }
    public Quaternion rot;
    public int x, y;
    public Sprite spr;
    public string objectstr;
}

public class s_levelloader : MonoBehaviour
{

    public s_grid Grid;
    public List<TextAsset> LevelData = new List<TextAsset>();
    public static string scenename;
    public int currentlevel = 0;
    public List<GameObject> gamobj = new List<GameObject>();
    public static s_levelloader load;
    public GameObject currlevel;
    public GameObject bas;
    public GameObject baselev;

    s_leveldat ldat;

    private void Awake()
    {
        scenename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString();
        if (load == null)
            load = this;
        currentlevel = s_mainmenu.loadedLevelNum;
    }

    public void LoadDat()
    {
        LoadData();
    }
    
    public void LoadData(string dire)
    {
        string te = "";
        te = File.ReadAllText(dire);
        Grid.ClearGrid();

        Debug.Log(dire);
        s_leveldat leveldata = JsonUtility.FromJson<s_leveldat>(te);
        Grid.groundworldsize = leveldata.gridsize;

        load.FeedGridData(leveldata);
        Grid.UnpaintAllNodes();
    }

    public void LoadData()
    {
        string te = "";
        te = LevelData[currentlevel].text;
        s_leveldat leveldata = JsonUtility.FromJson<s_leveldat>(te);
        Grid.groundworldsize = leveldata.gridsize;

        load.FeedGridData(leveldata);
        Grid.UnpaintAllNodes();
    }

    public void SaveData(string dir)
    {
        s_leveldat dat = new s_leveldat(Grid.character_layer, Grid.item_layer, Grid.block_layer, Grid.groundworldsize);

        string jsonString = JsonUtility.ToJson(dat);

        File.WriteAllText(dir, jsonString);
    }

    void FeedGridData(s_leveldat levelData)
    {
        foreach (s_nodedat block in levelData.nodes_blocks)
        {
            Grid.block_layer[block.x, block.y] = Grid.SpawnObject(block.objectstr, new Vector2(20 * block.x, 20 * block.y));

            //SpriteRenderer rend = Grid.block_layer[block.x, block.y].gameObject.GetComponent<SpriteRenderer>();

            /*
            if (rend != null)
                if (block.spr != null)
                    //rend.sprite = block.spr;

                    Grid.block_layer[block.x, block.y].transform.rotation = block.rot;
            */
        }
        foreach (s_nodedat cha in levelData.nodes_character)
        {
            o_character character =
                (o_character)Grid.SpawnObject(cha.objectstr, new Vector2(20 * cha.x, 20 * cha.y));
            if (character.GetComponent<ICharacter>() != null)
                character.GetComponent<ICharacter>().Intialize();
        }
        foreach (s_nodedat cha in levelData.nodes_items)
        {
            o_item it = (o_item)Grid.SpawnObject(cha.objectstr, new Vector2(20 * cha.x, 20 * cha.y));
            if (it != null)
                Grid.item_layer[cha.x, cha.y] = it;
        }

        for (int x = 0; x < Grid.gridworldsize.x; x++)
        {
            for (int y = 0; y < Grid.gridworldsize.y; y++)
            {
                s_nodedat block_nod = levelData.nodes_blocks.Find(obj => obj.x == x && obj.y == y);
                s_nodedat character_nod = levelData.nodes_character.Find(obj => obj.x == x && obj.y == y);
                s_nodedat item_nod = levelData.nodes_items.Find(obj => obj.x == x && obj.y == y);
            }
        }
        Grid.ResetNodes();
    }

    int CheckNameLevel(string level)
    {
        for (int s = 0; s < LevelData.Count; s++)
        {
            if (LevelData[s].name == level)
            {
                return s;
            }
        }
        return -1;
    }

    public void SwitchToGame()
    {
        //ldat = new s_leveldat(Grid.character_layer, Grid.block_layer, Grid.gridworldsize);
        Text text = GameObject.Find("SaveField").transform.GetChild(2).GetComponent<Text>();
        string filename = text.text;
        currentlevel = CheckNameLevel(filename);

        Scene sc = SceneManager.GetSceneByName("InGame");

        SceneManager.LoadScene("InGame");
        print("OK");

        /*
        if (scenename == "LevelEditor")
        {
            //
            //scenename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString();
        }
        else if (scenename == "InGame")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("LevelEditor");
            //scenename = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString();
        }*/
    }

}

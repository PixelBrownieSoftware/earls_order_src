using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class s_charactermanager : MonoBehaviour {

    public Text PlayerCount;
    public Text EnemyCount;
    public Text stats;

    public GUIStyle guithing;
    public GUISkin skin;
    public AudioClip hurtsound;
    public AudioClip select;

    public Texture2D actionBar;
    public Texture2D playerBar;
    public Texture2D enemyBar;
    public Texture2D emptyBar;

    public List<o_character> Players = new List<o_character>();
    public List<o_character> Enemies = new List<o_character>();
    public Queue<o_character> EnemyQueue = new Queue<o_character>();
    public List<string> Heroes = new List<string>();
    public List<string> Adversaries = new List<string>();

    public GameObject attkeffect;
    public s_grid Grid;
    public bool IsSkipped = false;

    public List<o_character> targets = new List<o_character>();
    o_character targetToAttack;

    s_object item_on_floor;   //Item on floor
    
    bool isattacking = false; //ATTACKING ANIMATION

    bool game_on = true;
    bool enable_skip_button = true;     //To prevent an eternal skip if no enemy can make a feesable move

    public o_character player;
    int pathfcost = 0;
    int selectChar = 0;

    public enum STATEMACHINE
    {
        IDLE,
        SELECT_CHAR,
        MOVE_TO,
        WALK,
        ATTACK_SELECT,
        ATTACK_EXECUTE,
        DONE
    }
    public STATEMACHINE STATES;

    enum VICTORY_DICTATOR
    {
        PLAYER,
        ENEMY,
        NONE
    }
    VICTORY_DICTATOR WINNER;
    public enum TURNS
    {
        PLAYER, ENEMY
    }
    public TURNS TURNSTATE;

    public Button FindButton(string name)
    {
        if (GameObject.Find(name) == null)
            return null;
        if (GameObject.Find(name).GetComponent<Button>() == null)
            return null;

        return GameObject.Find(name).GetComponent<Button>();
    }

    private void Awake()
    {
        Grid = GameObject.Find("Manager").GetComponent<s_grid>();
        stats = GameObject.Find("StatusText").GetComponent<Text>();
    }

    private void Start()
    {
        InitCharacters();
    }
    

    public void NormalSpeed()
    {
        Time.timeScale = 1;
    }

    void InitCharacters()
    {
        Players.Clear();
        Enemies.Clear();
        foreach (o_character c in Grid.character_layer)
        {
            if (c == null)
                continue;

            bool isEnemyMatch = Adversaries.Find(en => en == c.name) != null;
            bool isPlayerMatch = Heroes.Find(pl => pl == c.name) != null;

            if (PlayerPrefs.GetInt("MonsterMode") == 0)
            {
                if (isPlayerMatch)
                {
                    c.playable = true;
                    Players.Add(c);
                }
                else if (isEnemyMatch)
                {
                    c.playable = false;
                    Enemies.Add(c);
                }
            }
            else
            {
                if (isPlayerMatch)
                {
                    c.playable = false;
                    Enemies.Add(c);
                }
                else if (isEnemyMatch)
                {
                    c.playable = true;
                    Players.Add(c);
                }
            }
        }
    }

    public void ChangeTarget(int i)
    {
        selectChar += i;
        selectChar = Mathf.Clamp(selectChar, 0, targets.Count - 1);

        targetToAttack = targets[selectChar];
    }

    IEnumerator NextMap()
    {
        s_camera.staticCam.StartCoroutine(s_camera.staticCam.Fade(Color.black, 5));
        s_levelloader lo = s_levelloader.load;
        Grid.ClearGrid();
        game_on = false;
        if (lo.currentlevel == lo.LevelData.Count - 1)
        {
            PlayerPrefs.SetInt("GameCompleted", 1);
            PlayerPrefs.Save();
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        }

        lo.currentlevel++;
        int curlev = lo.currentlevel;

        PlayerPrefs.SetInt("CurrentLevel", curlev);
        PlayerPrefs.Save();
        yield return new WaitForSeconds(1f);
        s_camera.staticCam.StartCoroutine(s_camera.staticCam.Fade(Color.clear, 5));
        s_levelloader.load.LoadData();
        //Grid.Initialize();
        InitCharacters();
        game_on = true;
    }

    public void EndTurn()
    {
        if (GetWinner == VICTORY_DICTATOR.PLAYER)
        {
            StartCoroutine(NextMap());
            return;
        }
        if (TURNSTATE == TURNS.PLAYER)
        {
            foreach (o_character p in Enemies)
            {
                //for now they will recover all AP
                p.actionPoints = p.maxActionPoints;
            }
            EnemyTurn();
            TURNSTATE = TURNS.ENEMY;
        }
        else
        {
            foreach (o_character p in Players)
            {
                //for now they will recover all AP
                p.actionPoints = p.maxActionPoints;
            }
            IsSkipped = false;
            TURNSTATE = TURNS.PLAYER;
            STATES = STATEMACHINE.IDLE;
        }

    }

    public void PickUpItem()
    {
        player.current_item = (o_item)Grid.ObjectFromWorld(player.transform.position, 1);
        Grid.DespawnObject(Grid.ObjectFromWorld(player.transform.position, 1));

        STATES = STATEMACHINE.SELECT_CHAR;
    }
    
    public void DropItem()
    {
        Grid.SpawnObject(player.current_item.name , player.transform.position);
        player.current_item = null;
        STATES = STATEMACHINE.SELECT_CHAR;
    }

    public void UseItem()
    {
        //Play some anim
        o_item ite = player.current_item;
        switch (ite.ITEM_TYPE)
        {
            case o_item.IT_TYPE.AP:
                player.actionPoints += ite.amount; 
                break;

            case o_item.IT_TYPE.HEALTH:
                player.health += ite.amount;
                break;
        }
        player.health = Mathf.Clamp(player.health, 0, player.maxHealth);
        player.current_item = null;
        //Grid.DespawnObject(Grid.ObjectFromWorld(player.transform.position, 1));
        STATES = STATEMACHINE.SELECT_CHAR;
    }

    public void EnemyTurn()
    {
        foreach (o_character e in Enemies)
        {
            EnemyQueue.Enqueue(e);
        }
    }

    public void CheckCharacterSurroundings(bool is_enemy)
    {
        //If an enemy or two appears enable the confirm attack button
        targets.Clear();
        HashSet<o_node> enemynodes = Grid.CheckAroundNode(Grid.NodeFromWorld(player.transform.position));

        foreach (o_node e in enemynodes)
        {
            s_object enemyObject = Grid.ObjectFromWorld(e);

            if (enemyObject == null)
                continue;

            if (enemyObject.GetType() == typeof(o_character))
            {
                o_character enemy = enemyObject.GetComponent<o_character>();

                if (PlayerPrefs.GetInt("MonsterMode") == 0)
                {
                    if (!is_enemy)
                    {
                        if (!Heroes.Contains(enemy.name))
                            targets.Add(enemy);
                    }
                    else
                    {
                        if (!Adversaries.Contains(enemy.name))
                            targets.Add(enemy);
                    }
                }else 
                if (PlayerPrefs.GetInt("MonsterMode") == 1)
                {
                    if (!is_enemy)
                    {
                        if (!Adversaries.Contains(enemy.name))
                            targets.Add(enemy);
                    }
                    else
                    {
                        if (!Heroes.Contains(enemy.name))
                            targets.Add(enemy);
                    }
                }

            }
            else continue;
        }
    }

    public void LooseFocus()
    {
        STATES = STATEMACHINE.IDLE;
        player = null;
    } 

    public void ConfirmMovement()
    {
        player.actionPoints -= pathfcost;
        pathfcost = 0;

        if (!IsSkipped)
        {
            STATES = STATEMACHINE.WALK;
            Grid.UnpaintAllNodes();
            StartCoroutine(player.MoveToPosition(this));
        }
        else { //If the player is walking then stop.
            player.Teleport(this);
        }
        //Disable CancelMove and Confirm
    }

    public void ConfirmEnemyMovement()
    {
        Grid.UnpaintAllNodes();
        player.actionPoints -= pathfcost;
        StartCoroutine(player.MoveToPosition(this));
    }

    IEnumerator AttackingAnim()
    {
        isattacking = true;
        attkeffect.transform.position = targetToAttack.transform.position;
        attkeffect.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        attkeffect.SetActive(false);
        s_sound.PlaySound(hurtsound);

        for (int i = 0; i < 4; i++)
        {
            targetToAttack.renderer.color = Color.red;
            yield return new WaitForSeconds(0.02f);
            targetToAttack.renderer.color = Color.white;
        }
        targetToAttack.renderer.color = Color.white;
        AttackCalculations(targetToAttack);

        isattacking = false;
    }

    void AttackCalculations(o_character targ)
    {
        if (player == null)
            return;
        targ.health -= player.attack;
        player.actionPoints -= 2;
        player.health = Mathf.Clamp(player.health, 0, player.maxHealth);
        //actionPoints = Mathf.Clamp(actionPoints, 0, maxActionPoints);
    }
    
    public void SwitchStateOnClick(int e)
    {
        STATES = (STATEMACHINE)e;
    }
    
    VICTORY_DICTATOR GetWinner
    {
        get
        {
            int en = 0;
            int pl = 0;

            foreach (o_character c in Players)
                if (c.health >= 0)
                    pl++;
            foreach (o_character c in Enemies)
                if (c.health >= 0)
                    en++;

            if (pl == 0)
                return VICTORY_DICTATOR.ENEMY;
            else if (en == 0)
                return VICTORY_DICTATOR.PLAYER;
            else return VICTORY_DICTATOR.NONE;

        }
    }

    #region WRAPPERS
    bool PushButton(Rect rect, string text) //A wrapper for playing the sound and having the guistyle
    {
        if (GUI.Button(rect, text, skin.GetStyle("Box")))
        {
            //s_sound.PlaySound(select);
            return true;
        }
        return false;
    }

    public bool CheckForObject(Vector3 mousePositon)//A wrapper function for checking if a certain object exists
    {
        if (Grid.NodeFromWorld(mousePositon) != null)

            if (Grid.ObjectFromWorld(mousePositon) != null)
                return true;

        return false;
    }

    public s_object GetObjectFromWorld(Vector3 mousePositon)
    {
        Vector2 snap = Grid.SnapToGrid(mousePositon);
        s_object obj = Grid.ObjectFromWorld(snap);
        if (obj != null)
            return obj;
        else
            return null;
    }

    public bool IsCharacter(Vector3 mousePositon, s_object obj) {
        if (obj != null)
        {
            if (obj.GetType() == typeof(o_character))
            {
                return true;
            }
        }
        return false;
    }

    #endregion

    void DrawBar()
    {
        for (int i = 0; i < player.maxHealth; i++)
        {
            Color guiCol = Color.white;
            if (player.health <= i)
            {
                GUI.DrawTexture(new Rect(i * emptyBar.width, 120, emptyBar.width, emptyBar.height), emptyBar);
                continue;
            }

            GUI.color = guiCol;
            if (player.playable)
                GUI.DrawTexture(new Rect(i * playerBar.width, 120, playerBar.width, playerBar.height), playerBar);
            else
                GUI.DrawTexture(new Rect(i * enemyBar.width, 120, enemyBar.width, enemyBar.height), enemyBar);
        }

        for (int i = 0; i < player.maxActionPoints; i++)
        {
            Color guiCol = Color.white;

            if (player.actionPoints - pathfcost <= i)
                guiCol = Color.magenta;

            if(STATES == STATEMACHINE.ATTACK_SELECT)
            if (player.actionPoints - 2 <= i)
                guiCol = Color.magenta;

            if (pathfcost > player.actionPoints)
                guiCol = Color.grey;

            if (player.actionPoints <= i)
            {
                guiCol = Color.white;
                GUI.color = guiCol;
                GUI.DrawTexture(new Rect(i * emptyBar.width, 120 + actionBar.height, emptyBar.width, emptyBar.height), emptyBar);
                continue;
            }

            GUI.color = guiCol;
            GUI.DrawTexture(new Rect(i * actionBar.width, 120 + actionBar.height, actionBar.width, actionBar.height), actionBar);
        }
    }

    void DrawTargetBar()
    {
        for (int i = 0; i < targetToAttack.maxHealth; i++)
        {
            Color guiCol = Color.white;

            if (targetToAttack.health - player.attack <= i)
                guiCol = Color.magenta;

            if (targetToAttack.health <= i)
            {
                guiCol = Color.white;
                GUI.color = guiCol;
                GUI.DrawTexture(new Rect(i * emptyBar.width, 210, emptyBar.width, emptyBar.height), emptyBar);
                continue;
            }

            GUI.color = guiCol;

            GUI.DrawTexture(new Rect(i * enemyBar.width, 210, enemyBar.width, enemyBar.height), enemyBar);
        }
    }

    private void OnGUI()
    {
        if (player != null)
            if (TURNSTATE == TURNS.PLAYER)
                DrawBar();

        if (game_on)
        {
            switch (STATES)
            {
                case STATEMACHINE.IDLE:

                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:

                            if (GetWinner == VICTORY_DICTATOR.PLAYER)
                            {
                                if (PushButton(new Rect(90, 90, 125, 30), "Claim Victory"))
                                    EndTurn();
                            }
                            else
                            if (PushButton(new Rect(90, 90, 90, 30), "End Turn"))
                                EndTurn();
                            break;
                    }

                    break;

                case STATEMACHINE.SELECT_CHAR:
                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:
                            if (Players.Contains(player))
                            {
                                item_on_floor = Grid.ObjectFromWorld(player.transform.position, 1);
                                if (item_on_floor != null)
                                {
                                    if (player.current_item == null)
                                    {
                                        if (PushButton(new Rect(5, 180, 115, 30), "Pick up item"))
                                        {
                                            PickUpItem();
                                            s_sound.PlaySound(select);
                                        }
                                    }
                                }
                                if (player.current_item != null)
                                {
                                    if (PushButton(new Rect(5, 180, 90, 30), "Use item"))
                                        UseItem();
                                    if (item_on_floor == null)
                                        if (PushButton(new Rect(5, 210, 90, 30), "Drop item"))
                                            DropItem();
                                }
                                

                            }
                            if (Input.GetMouseButtonDown(1))
                                LooseFocus();
                            break;
                            
                    }

                    //Have a cancel button where this player is no longer the focus

                    //Attack button where it checks enemies

                    break;
                    

                case STATEMACHINE.ATTACK_SELECT:

                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:

                            DrawTargetBar();

                            break;
                            
                    }

                    break;


            }
        }
    }

    private void Update()
    {
        if (PlayerPrefs.GetInt("MonsterMode") == 0)
        {
            if (Players.Count < 10)
                PlayerCount.text = "0" + Players.Count;
            else
                PlayerCount.text = "" + Players.Count;

            if (Enemies.Count < 10)
                EnemyCount.text = "0" + Enemies.Count;
            else
                EnemyCount.text = "" + Enemies.Count;
        }
        else
        {
            if (Players.Count < 10)
                PlayerCount.text = "0" + Enemies.Count;
            else
                PlayerCount.text = "" + Enemies.Count;

            if (Enemies.Count < 10)
                EnemyCount.text = "0" + Players.Count;
            else
                EnemyCount.text = "" + Players.Count;
        }

        Vector3 mousePositon = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (player == null)
        {
            pathfcost = 0;
            stats.text = "Nothing";
        }

        if (GetWinner == VICTORY_DICTATOR.ENEMY)
        {
            s_camera.staticCam.StartCoroutine(s_camera.staticCam.Fade(Color.black, 0.3f));

            game_on = false;
            if (s_camera.staticCam.isfaded)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
            }
        }
        

        if (game_on)
        {
            switch (STATES)
            {
                case STATEMACHINE.IDLE:
                    
                    switch (TURNSTATE)
                    {
                        
                        case TURNS.PLAYER:
                            if (Input.GetMouseButtonDown(0))
                            {
                                if (CheckForObject(mousePositon))
                                {
                                    s_object obj = GetObjectFromWorld(mousePositon);
                                    if (IsCharacter(mousePositon,obj))
                                    {
                                        o_character obselect = obj.GetComponent<o_character>();
                                        if (obselect.health > 0)
                                        {
                                            if (obselect.playable)
                                            {
                                                pathfcost = 0;
                                                SwitchCharacter((o_character)obj);
                                                CheckCharacterSurroundings(false);
                                            }
                                            player = obselect;
                                            STATES = STATEMACHINE.SELECT_CHAR;
                                        }
                                    }
                                    
                                }
                            }
                            break;
                        #region ENEMY
                        case TURNS.ENEMY:
                            
                            if (EnemyQueue.Count > 0)
                            {
                                //print("Chara " + EnemyQueue.Peek());
                                player = EnemyQueue.Peek();
                                //print("pl " + player);
                                STATES = STATEMACHINE.SELECT_CHAR;
                            }
                            else
                            {
                                s_camera.staticCam.StartCoroutine(s_camera.staticCam.Fade(Color.clear, 4));
                                if (s_camera.staticCam.isfaded)
                                {
                                    print("Progress END: " + EnemyQueue.Count);
                                    EndTurn();
                                }
                            }

                            break;
                            #endregion
                    }

                    break;

                case STATEMACHINE.SELECT_CHAR:

                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:

                            if (player.current_item != null)
                                stats.text = player.name + "\n" + "Item: " + player.current_item.name;
                            else
                                stats.text = player.name;

                            //actionpoints = "AP: " + player.actionPoints + "/" + player.maxActionPoints;

                            if (CheckForObject(mousePositon))
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    CheckCharacterSurroundings(false);
                                    s_object obj = GetObjectFromWorld(mousePositon);

                                    if (obj != null)
                                    {
                                        if (obj.GetType() == typeof(o_character))
                                        {
                                            o_character chara = obj.GetComponent<o_character>();
                                            if (targets.Contains(chara))
                                            {
                                                if (!chara.playable)
                                                {
                                                    targetToAttack = chara;
                                                    STATES = STATEMACHINE.ATTACK_SELECT;
                                                }
                                            }
                                        }
                                        
                                    }
                                }
                            }

                            if (Input.GetMouseButtonDown(0))
                            {
                                Vector2 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                                if (player.MoveCost(mousepos) != -1)
                                {
                                    Grid.UnpaintAllNodes();
                                    pathfcost = player.MoveCost(mousepos);
                                    player.SetDirections(Grid.NodeFromWorld(mousepos));
                                    List<o_node> dir = player.directions;

                                    foreach (o_node d in dir)
                                    {
                                        Grid.PaintNode(Grid.NodePositionFromWorld(d), Color.yellow);
                                    }
                                    STATES = STATEMACHINE.MOVE_TO;
                                }
                                

                            }

                            break;
                        #region ENEMY
                        case TURNS.ENEMY:

                            if (player.current_item != null)
                            {
                                UseItem();
                            }

                            /*if (targetToAttack != null)
                                if (targetToAttack.health == 0 && !targetToAttack.IsDisappear())
                                    return;
                                    */
                            CheckCharacterSurroundings(true);
                            if (targets.Count == 0)
                                STATES = STATEMACHINE.MOVE_TO;
                            else
                                STATES = STATEMACHINE.ATTACK_SELECT;
                            break;
                            #endregion
                    }

                    //Have a cancel button where this player is no longer the focus

                    //Attack button where it checks enemies

                    break;

                case STATEMACHINE.MOVE_TO:
                    
                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:
                            if (Input.GetMouseButtonDown(0))
                            {

                                Vector2 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                                if (player.playable)
                                {
                                    if (player.actionPoints >= pathfcost)
                                    {
                                        if (Grid.NodeFromWorld(mousepos) == player.directions[player.directions.Count - 1])
                                        {
                                            ConfirmMovement();
                                            return;
                                        }
                                    }
                                }

                                if (player.MoveCost(mousepos) != -1)
                                {
                                    Grid.UnpaintAllNodes();
                                    pathfcost = player.MoveCost(mousepos);
                                    player.SetDirections(Grid.NodeFromWorld(mousepos));
                                    List<o_node> dir = player.directions;
                                    
                                    foreach (o_node d in dir)
                                    {
                                        Grid.PaintNode(Grid.NodePositionFromWorld(d), Color.yellow);
                                    }
                                }
                            }
                            
                            if (Input.GetMouseButtonDown(1))
                            {
                                Grid.UnpaintAllNodes();
                                LooseFocus();
                            }

                            break;
                        #region ENEMY
                        case TURNS.ENEMY:

                            int newcost = int.MaxValue;
                            o_character tar = null;
                            
                            foreach (o_character e in Players)
                            {
                                //Check around the target and pathfind for each position.
                                HashSet<o_node> aroundChar = Grid.CheckAroundNode(Grid.NodeFromWorld(e.transform.position));

                                int potentialMoveCost = int.MaxValue;
                                e.GetComponent<SpriteRenderer>().color = Color.red;
                                o_node nof = null;

                                foreach (o_node no in aroundChar)
                                {
                                    if (!no.walkable)
                                        continue;
                                    
                                    int comp = player.MoveCost(no.position);
                                    if (comp == -1)
                                        continue;

                                    potentialMoveCost = Mathf.Min(potentialMoveCost, comp);
                                    if (potentialMoveCost == comp)
                                    {
                                        nof = no;
                                    }
                                }

                                e.GetComponent<SpriteRenderer>().color = Color.white;
                                if (potentialMoveCost <= player.range)
                                {
                                    if (potentialMoveCost <= player.actionPoints)
                                    {
                                        tar = e;
                                        newcost = Mathf.Min(newcost, potentialMoveCost);
                                        if (newcost == potentialMoveCost)
                                        {
                                            if (nof != null)
                                                player.SetDirections(nof);
                                        }
                                    }
                                }
                                else
                                    continue;
                                Grid.UnpaintAllNodes();

                            }

                            if (newcost != int.MaxValue)
                            {
                                Grid.UnpaintAllNodes();
                                pathfcost = newcost;
                                if (pathfcost > 0)
                                    ConfirmMovement();
                                else
                                    NextEnemy();
                            }
                            else
                                NextEnemy();

                            break;
                            #endregion
                    }
                    //Confirm?
                    //Cancecl Move

                    break;

                case STATEMACHINE.ATTACK_SELECT:

                    switch (TURNSTATE)
                    {
                        case TURNS.PLAYER:
                            

                            if (targetToAttack != null)
                                targetToAttack.renderer.color = Color.magenta;

                            if (Input.GetMouseButtonDown(0))
                            {
                                s_object obj = GetObjectFromWorld(mousePositon);
                                if (obj != targetToAttack)
                                {
                                    o_character newtarg = targets.Find(x => obj == x);

                                    if (newtarg != null)
                                    {
                                        targetToAttack.renderer.color = Color.white;
                                        targetToAttack = newtarg;
                                        return;
                                    }
                                }

                            }


                            if (targetToAttack.health > 0 && player.actionPoints >= 2)
                            {
                                if (Input.GetMouseButtonDown(0))
                                {
                                    targetToAttack.renderer.color = Color.white;
                                    STATES = STATEMACHINE.ATTACK_EXECUTE;
                                }
                            }

                            if (targetToAttack.health <= 0 || player.actionPoints < 2)
                            {
                                targetToAttack.renderer.color = Color.white;
                                LooseFocus();
                            }

                            if (Input.GetMouseButtonDown(1))
                            {
                                if (targetToAttack != null)
                                    targetToAttack.renderer.color = Color.white;
                                LooseFocus();
                            }
                            break;

                        #region ENEMY
                        case TURNS.ENEMY:

                            CheckCharacterSurroundings(true);
                            if (targets.Count > 0)
                            {
                                //The enemy can attack the player again if they have sufficent points
                                if (player.actionPoints >= 2)
                                {
                                    targetToAttack = GetTargetWithLowestHealth();

                                    if (targetToAttack.health > 0)
                                    {
                                        if (!isattacking && !IsSkipped)
                                            StartCoroutine(AttackingAnim());

                                        if (IsSkipped)
                                            AttackCalculations(targetToAttack);
                                    }
                                }
                                else 
                                    NextEnemy();
                            }
                            else
                                STATES = STATEMACHINE.SELECT_CHAR;

                            break;
                            #endregion
                    }

                    break;

                case STATEMACHINE.ATTACK_EXECUTE:
                    
                    if (!isattacking)
                        StartCoroutine(AttackingAnim());
                    else if (TURNSTATE == TURNS.PLAYER)
                        STATES = STATEMACHINE.ATTACK_SELECT;

                    break;

                case STATEMACHINE.WALK:
                    //Call Player enumarator to walk
                    //Once done go back to select char

                    break;

                    
            }
        }
    }

    void NextEnemy()
    {
        player = null;
        EnemyQueue.Dequeue();
        STATES = STATEMACHINE.IDLE;
        //print("Progress: " + EnemyQueue.Count);
        
    }

    o_character GetTargetWithLowestHealth()
    {
        int lasthealth = int.MaxValue;
        o_character chara = null;
        foreach (o_character t in targets)
        {
            if (lasthealth > t.health)
            {
                lasthealth = t.health;
                chara = t;
            }

        }
        return chara;
    }

    public void SwitchCharacter(o_character character)
    {
        player = character;
    }


}

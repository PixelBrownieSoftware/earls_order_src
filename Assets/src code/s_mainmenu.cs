using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class s_mainmenu : MonoBehaviour {
    
    public GUISkin skin;
    const int y_space = 55;
    const float y_offset = 1.6f;
    public static int loadedLevelNum = 0;
    s_camera cam;
    bool confirmed = false;
    public SpriteRenderer instruction;
    bool instr_activated = false;
    enum MENUSTATE { MAIN, INSTRUCTIONS, ERASE }
    MENUSTATE MENU;
    
    private void Awake()
    {
        cam = GetComponent<s_camera>();
        Screen.SetResolution(896, 504, false);
    }
    bool PushButton(Rect rect, string text) //A wrapper for playing the sound and having the guistyle
    {
        if (GUI.Button(rect, text, skin.GetStyle("Box")))
        {
            //s_sound.PlaySound(select);
            return true;
        }
        return false;
    }

    void MonsterMode()
    {
        confirmed = true;
        loadedLevelNum = 0;
        PlayerPrefs.SetInt("CurrentLevel", 1);
        PlayerPrefs.SetInt("MonsterMode", 1);
        cam.StartCoroutine(cam.Fade(Color.black, 4)); 
    }

    void StartGame()
    {
        confirmed = true;
        loadedLevelNum = 0;
        PlayerPrefs.SetInt("MonsterMode", 0);
        cam.StartCoroutine(cam.Fade(Color.black, 4));   
    }

    void Continue()
    {
        confirmed = true;
        loadedLevelNum = PlayerPrefs.GetInt("CurrentLevel");
        cam.StartCoroutine(cam.Fade(Color.black, 4));
    }

    void resetstats()
    {
        PlayerPrefs.SetInt("GameCompleted", 0);
        PlayerPrefs.SetInt("CurrentLevel", 0);
        PlayerPrefs.SetInt("MonsterMode", 0); 
    }

    private void OnGUI()
    {
        float i = 1 + y_offset; //For spacing
        if (!confirmed)
        {
            switch (MENU)
            {
                case MENUSTATE.MAIN:
                    if (PushButton(new Rect(20, y_space * i, 135, 40), "Start Game"))
                    {
                        StartGame();
                    }
                    i++;

                    if (PushButton(new Rect(20, y_space * i, 135, 40), "Instructions"))
                    {
                        instruction.enabled = true;
                        MENU = MENUSTATE.INSTRUCTIONS;
                    }
                    i++;

                    if (PlayerPrefs.GetInt("GameCompleted") == 1)
                    {
                        if (PushButton(new Rect(20, y_space * i, 135, 40), "Monster Mode"))
                        {
                            MonsterMode();
                        }
                        i++;
                    }
                    if (PlayerPrefs.GetInt("CurrentLevel") > 0)
                    {
                        if (PushButton(new Rect(20, y_space * i, 135, 40), "Continue"))
                        {
                            Continue();
                        }
                        i++;
                    }

                    if (PushButton(new Rect(20, y_space * i, 135, 40), "Erase Data"))
                    {
                        MENU = MENUSTATE.ERASE;
                    }
                    i++;
                    break;

                case MENUSTATE.INSTRUCTIONS:
                    if (PushButton(new Rect(20, y_space * i, 135, 40), "Back"))
                    {
                        instruction.enabled = false;
                        MENU = MENUSTATE.MAIN;
                    }
                    break;

                case MENUSTATE.ERASE:
                    GUI.Box(new Rect(20, y_space * i, 135, 40), "Erase data?", skin.GetStyle("Box"));

                    i++;
                    if (PushButton(new Rect(20, y_space * i, 135, 40), "Yes"))
                    {
                        resetstats();
                        MENU = MENUSTATE.MAIN;
                    }
                    i++;
                    if (PushButton(new Rect(20, y_space * i, 135, 40), "No"))
                    {
                        MENU = MENUSTATE.MAIN;
                    }
                    break;

            }
            

        }
        else
        {
            if (cam.isfaded)
                SceneManager.LoadScene("InGame");
        }
        i = 0;

    }
}

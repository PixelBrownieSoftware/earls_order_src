using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_debug : MonoBehaviour {

    public AudioClip introjingle;
    public SpriteRenderer endingimg;
    public GUISkin skin;
    public string sceneString;

    public Sprite[] images;

    bool PushButton(Rect rect, string text) //A wrapper for playing the sound and having the guistyle
    {
        if (GUI.Button(rect, text, skin.GetStyle("Box")))
        {
            //s_sound.PlaySound(select);
            return true;
        }
        return false;
    }

    private void Awake()
    {
        sceneString = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToString();
        endingimg = GetComponent<SpriteRenderer>();
        if (endingimg != null && sceneString == "Ending")
        {
            if (PlayerPrefs.GetInt("MonsterMode") == 0)
            {
                endingimg.sprite = images[0];
            }
            else
            {
                endingimg.sprite = images[1];
            }
        }
    }

    private void ChangeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    public void PlayIntroJingle()
    {
        s_sound.PlaySound(introjingle);
    }

    private void OnGUI()
    {
        if (sceneString == "Ending")
        {
            if(PushButton(new Rect(670, 446, 130, 30), "Back to Title"))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
            }
        }
    }

}

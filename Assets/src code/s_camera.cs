using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class s_camera : MonoBehaviour {

    public static s_camera staticCam;
    Vector3 camerapos;
    public bool isfaded { get; set; }
    public bool fading { get; set; }
    Image Fadeimg;
    public bool Control;

    private void Awake()
    {
        isfaded = false;
        fading = false;
        staticCam = this;
        if (GameObject.Find("Fade"))
        {
            Fadeimg = GameObject.Find("Fade").GetComponent<Image>();
            StartCoroutine(Fade(Color.clear, 4));
        }
    }

    void Update () {

        if (Control)
        {
            if (Input.GetKey(KeyCode.A))
            {
                camerapos.x -= 4;
            }
            if (Input.GetKey(KeyCode.D))
            {
                camerapos.x += 4;
            }
            if (Input.GetKey(KeyCode.W))
            {
                camerapos.y += 4;
            }
            if (Input.GetKey(KeyCode.S))
            {
                camerapos.y -= 4;
            }

            camerapos.x = Mathf.Clamp(camerapos.x, 0, 500);
            camerapos.y = Mathf.Clamp(camerapos.y, 100, 750);
        }
        camerapos.z = -10;
        transform.position = camerapos;
    }

    public IEnumerator Fade(Color colour, float speed)
    {
        isfaded = false;
        fading = true;
        Color former = Fadeimg.color;
        float t = 0f;
        while (Fadeimg.color != colour)
        {
            t += 0.1f;
            Fadeimg.color = Color.Lerp(former, colour, t);
            yield return new WaitForSeconds(Time.deltaTime / speed);
        }
        isfaded = true;
        fading = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KonamiCode : MonoBehaviour
{
    //Konami
    [SerializeField]
    private float timerKonamiMax;
    private float timerKonami;
    private int indexKonami;
    private KeyCode[] keysKonami;
    public UnityEvent KonamiCodeEvent = new UnityEvent();

    void Start()
    {
        //Konami
        timerKonami = timerKonamiMax;
        indexKonami = 0;
        keysKonami = new KeyCode[]
        {
            KeyCode.UpArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.DownArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.LeftArrow,
            KeyCode.RightArrow,
            KeyCode.B,
            KeyCode.A,
        };
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(keysKonami[indexKonami]))
            {
                indexKonami++;
                timerKonami = timerKonamiMax;
            }
            else
            {
                indexKonami = 0;
                timerKonami = timerKonamiMax;
            }
        }
        if (indexKonami == keysKonami.Length)
        {
            indexKonami = 0;
            KonamiCodeEvent?.Invoke();
            Debug.Log("KONAMI");
        }
        if (indexKonami > 0)
        {
            timerKonami -= Time.deltaTime;
        }
        if (timerKonami < 0)
        {
            indexKonami = 0;
            timerKonami = timerKonamiMax;
        }
    }
}

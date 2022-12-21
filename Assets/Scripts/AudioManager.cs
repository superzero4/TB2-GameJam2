using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //Liste des sons
    [SerializeField]
    public List<Sound> L_Sounds;

    void Start()
    {
        //Ajouts des sources
        foreach (Sound s in L_Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Play(string Name)
    {
        Sound s = L_Sounds.Find(sound => sound.Name == Name);
        s.source.Play();
    }

    public void Stop(string Name)
    {
        Sound s = L_Sounds.Find(sound => sound.Name == Name);
        s.source.Stop();
    }
}

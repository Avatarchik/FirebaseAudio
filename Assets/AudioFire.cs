using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFire : MonoBehaviour
{
    public AudioSource MusicSource;
    public string Key;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActive(bool b) {
        this.gameObject.SetActive(b);
    }

    public string RayCast() {
        return Key;
    }

}

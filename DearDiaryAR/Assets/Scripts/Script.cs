using UnityEngine;

public class Script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Narration>().StartTyping("Gooya Goooya Cachun Cachun Ra Ra");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

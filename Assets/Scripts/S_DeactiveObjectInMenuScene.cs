using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class S_DeactiveObjectInMenuScene : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Scene[] scenes = SceneManager.GetAllScenes();
        foreach (var scene in scenes)
        {
            if (scene.name == "MainMenu")
            {
                this.gameObject.SetActive(false);
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

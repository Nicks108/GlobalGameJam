using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class S_MainMenuUIController : MonoBehaviour
{
    private Button btnStart;
    private VisualElement root;

    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        btnStart = root.Q<Button>("btnStart");
        btnStart.clicked += LoadLevel;

        SceneManager.sceneLoaded += FixOcclusionCulling;
        SceneManager.LoadScene("MainLevel", LoadSceneMode.Additive);
        
    }

    private static void FixOcclusionCulling(Scene scene, LoadSceneMode mode)
    {
        if (scene.name== "MainLevel")
        {
            SceneManager.SetActiveScene(scene);
            Debug.Log("Activateing Scene "+SceneManager.GetActiveScene().name);
        }
        
    }

    // Update is called once per frame
    void LoadLevel()
    {
        SceneManager.LoadScene("MainLevel");
    }
}

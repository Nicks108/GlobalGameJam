using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOReferences : MonoBehaviour
{
    public static GOReferences GOReferencesInstance;
    public GameObject go_player;

    private void Awake()
    {
        GOReferencesInstance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

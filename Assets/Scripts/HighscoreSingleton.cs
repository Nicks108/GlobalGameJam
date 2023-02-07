using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Highscore", menuName = "Highscore", order = 1)]
public class HighscoreSingleton : ScriptableObject
{
    public string scoreName;
    public float highscore;
}

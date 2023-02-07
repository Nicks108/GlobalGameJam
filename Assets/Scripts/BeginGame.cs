using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class BeginGame : MonoBehaviour
{
    public Animator titleAnimation, anyKeyAnimation;
    public AnimationClip animSequence;
    public TextMeshProUGUI pressStart, highscoreText, scoreText, nameText;
    public GameObject[] onionList;
    public Transform spawnPoint;
    public HighscoreSingleton highscoreValues;
    bool beginGame;
    // Start is called before the first frame update
    IEnumerator Start()
    {
        beginGame = false;
        yield return new WaitUntil(() => titleAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
        scoreText.text = highscoreValues.highscore.ToString();
        nameText.text = highscoreValues.scoreName.ToString();
        pressStart.enabled = highscoreText.enabled = scoreText.enabled = nameText.enabled = true;
        beginGame = true;
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Reached");
        for (int i = 0; i < onionList.Length; i++)
        {
            onionList[i].GetComponent<Rigidbody>().velocity = Vector3.zero;
            onionList[i].transform.position = spawnPoint.position;
            //onionList[i].transform.rotation = spawnPoint.rotation;
        }
    }

    IEnumerator BeginGameAnimation()
    {
        anyKeyAnimation.SetTrigger("TriggerMainGame");// ay(animSequence.ToString());
        
        yield return new WaitWhile(() => anyKeyAnimation.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void StartTransitionAnimation()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    // Update is called once per frame
    void Update()
    {
        if(!beginGame)
        {
            return;
        }
        if(Input.anyKeyDown)
        {
            anyKeyAnimation.SetTrigger("TriggerMainGame");
            //StartCoroutine(BeginGameAnimation());
            beginGame = false;
        }
    }
}

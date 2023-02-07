using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
//using UnityEngine.Windows;

public class UIManager : MonoBehaviour
{
    [SerializeField]  public  UnityEvent spawnEvent;
    
    public Slider plantSpawnBar;
    public Image fillArea;
    public float amount, speed, startY, scoreY, scoreShakeVal;
    float spawnTick, runtimeVal = 1.0f;
    public bool beginBarTimer, initialEnteringStage;
    private int LetterIndex = 0;
    public GameObject activateSpawnBar;
    public TextMeshProUGUI ScoreText;
    public AudioSource EnergyMeterFullAudioSource;

    [Range(0,5)]
    public float EnergyBarLerpTime = 8f;
    public float energyBarDowntime = 10f;
    private float currentTime = 0f;
    float fontSizeJuice;
    bool displayStylishText;
    public float score, highscore, displayStylish, scoreIncreaseRate;
    Vector3 originalScorePosition;
    Vector2 juicyScaleValues, originalScaleValues;
    public BlastVegetables blastVegRef;

    public TextMeshProUGUI timerText, game, over, firstIn, secondIn, thirdIn;
    public TextMeshProUGUI[] InitialsTextMeshes;
    public GameObject panel;
    public float gameTimer;


public HighscoreSingleton highScoreUpdate;
    // Start is called before the first frame update
    void Start()
    {
        highscore = highScoreUpdate.highscore;
        initialEnteringStage = false;

        //originalScaleValues = tempText.rectTransform.sizeDelta;
        //fontSizeJuice = 380.6f;
        //juicyScaleValues.x = 977.8667f;
        //juicyScaleValues.y = 244.47f;
        scoreIncreaseRate = 1.5f;
        displayStylish = 0;
        score = 0;
        startY = plantSpawnBar.transform.localPosition.y;
        scoreY = ScoreText.transform.localPosition.y;
        amount = speed = 0f;
        scoreShakeVal = 5.0f;
        beginBarTimer = false;
        runtimeVal = 1.0f;
        InvokeRepeating("UpdateTimer", 1, 1);
        //StartCoroutine(ScrollToMax());
    }

    void UpdateTimer()
    {
        Debug.Log("Game timer is: "+gameTimer);
        if(gameTimer == 0)
        {
            Debug.Log("Timer is 0");
            CancelInvoke();
            GameOverSequence();
            return;
        }
        gameTimer -= 1;
        timerText.text = gameTimer.ToString();
    }

    void GameOverSequence()
    {
        S_PlayerControler.canMove = false;
        StartCoroutine(DisplayGameOver());
        initialEnteringStage = true;
        Debug.Log(initialEnteringStage);
    }

    IEnumerator DisplayGameOver()
    {
        panel.SetActive(true);
        game.enabled = true;
        yield return new WaitForSeconds(0.6f); // tweak?
        over.enabled = true;
        yield return new WaitForSeconds(0.5f);
        
        firstIn.enabled = true;
        secondIn.enabled = true;
        thirdIn.enabled = true;
    }

    char[] InputKey = new char[3];


 

    

    public void FlowerScore()
    {
        displayStylishText = true;
        score += Random.Range(5, 50);
    }

    public void OnAddScore(int Score)
    {
        score += Score; 
    }

    IEnumerator ScrollToMax()
    {
        while(!IsAtMax())
        {
            AdjustBar(0.1f);
            yield return new WaitForEndOfFrame();
        }
    }

    bool IsAtMax()
    {
        return plantSpawnBar.value == 1f ? true : false;
    }

    private bool CanPlayEnergyFullSound = true;
    public void AdjustBar(float adjustor)
    {
        if(beginBarTimer)
        {
            return;
        }
        plantSpawnBar.value += adjustor;
        if (IsAtMax() && CanPlayEnergyFullSound)
        {
            CanPlayEnergyFullSound = false;
            EnergyMeterFullAudioSource.Play();
            amount = speed = 0f;
            activateSpawnBar.SetActive(true);
            Debug.Log("PLayed Energy bar full audio");
        }
    }

    public AnimationCurve XDisplacment;
    public AnimationCurve YDisplacment;

    void ShakeBar(float speed, float amount)
    {
        if(speed == 0 || amount == 0)
        {
            return;
        }

        Vector3 tempVector = plantSpawnBar.transform.localPosition;
        tempVector.x = Mathf.Sin(Time.time * speed) * amount * XDisplacment.Evaluate(Time.time);
        tempVector.y = Mathf.Cos(Time.time * speed) * amount * YDisplacment.Evaluate(Time.time);
        float diff = startY + tempVector.y;
        tempVector.y = diff;
        //plantSpawnBar.transform.localPosition = Vector3.Lerp(plantSpawnBar.transform.localPosition, tempVector,0.9f);
        plantSpawnBar.transform.localPosition = tempVector;
    }

    void DetermineShakeLevel()
    {
        if((plantSpawnBar.value / plantSpawnBar.maxValue * 100) < 60)
        {
            fillArea.color = Color.red;
            amount = speed = 0f;
            return;
        }
        if ((plantSpawnBar.value / plantSpawnBar.maxValue * 100) >= 60)
        {
            amount = speed = 5.0f;
            fillArea.color = Color.yellow;
        }
        if ((plantSpawnBar.value / plantSpawnBar.maxValue * 100) >= 70)
        {
            amount = 5.0f;
            speed = 10.0f;
        }
        if ((plantSpawnBar.value / plantSpawnBar.maxValue * 100) >= 80)
        {
            fillArea.color = Color.green;
            amount = 10.0f;
            speed = 20.0f;
        }
        if ((plantSpawnBar.value / plantSpawnBar.maxValue * 100) >= 90)
        {
            amount = 15.0f;
            speed = 45.0f;
        }
    }

    public void ActivateSpawner()
    {
        beginBarTimer = true;
        spawnEvent.Invoke();
        activateSpawnBar.SetActive(false);
    }

    void EnterInitials()
    {
        if (Input.anyKeyDown)
        {
           if (Regex.IsMatch(Input.inputString, @"^[a-zA-Z]+$"))
            {
                InputKey[LetterIndex] = Input.inputString[0];

                InitialsTextMeshes[LetterIndex].text = InputKey[LetterIndex].ToString().ToUpper();
                LetterIndex++;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        ScoreText.text = Mathf.Round(score).ToString();
        if (beginBarTimer)
        {
            if (currentTime <= energyBarDowntime)
            {
                currentTime += Time.deltaTime;
                plantSpawnBar.value = Mathf.Lerp(1, 0, currentTime / EnergyBarLerpTime);
            }
            else
            {
                currentTime = 0f;
                beginBarTimer = false;
                CanPlayEnergyFullSound = true;
            }
            Debug.Log(currentTime);

        }
        else
        {
            DetermineShakeLevel();
            ShakeBar(speed, amount);
        }
        if (initialEnteringStage)
        {
            EnterInitials();
            Debug.Log("Any key down: " + Input.anyKeyDown);
        }
        if (!displayStylishText)
        {
            return;
        }
        if (displayStylish < score)
        {
            displayStylish += (scoreIncreaseRate * Time.deltaTime) * (score - displayStylish);
            if (displayStylish >= score)
            {
                displayStylish = score;
            }
            //ScoreText.text = Mathf.Round(displayStylish).ToString();
            Vector3 textVector = ScoreText.rectTransform.localPosition;
            textVector.x = Mathf.Sin(Time.time * scoreIncreaseRate) * scoreShakeVal * XDisplacment.Evaluate(Time.time);
            textVector.y = Mathf.Cos(Time.time * scoreIncreaseRate) * scoreShakeVal * YDisplacment.Evaluate(Time.time);
            //tempText.fontSize = fontSizeJuice;
            //tempText.rectTransform.sizeDelta = juicyScaleValues;
            float diff = scoreY + textVector.y;
            textVector.y = diff;
            //plantSpawnBar.transform.localPosition = Vector3.Lerp(plantSpawnBar.transform.localPosition, tempVector,0.9f);
            ScoreText.transform.localPosition = textVector;
        }
        else
        {
            //tempText.rectTransform.position = originalScorePosition;
            //tempText.rectTransform.sizeDelta = originalScaleValues;
            displayStylishText = false;
        }

        
    }
}
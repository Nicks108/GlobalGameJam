using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
public class UIManager : MonoBehaviour
{
    [SerializeField] UnityEvent spawnEvent;
    public Slider plantSpawnBar;
    public Image fillArea;
    public float amount, speed, startY;
    float spawnTick, runtimeVal;
    bool beginBarTimer;
    public GameObject activateSpawnBar;
    // Start is called before the first frame update
    void Start()
    {
        startY = plantSpawnBar.transform.localPosition.y;
        amount = speed = 0f;
        beginBarTimer = false;
        runtimeVal = 1.0f;
        //StartCoroutine(ScrollToMax());
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

    public void AdjustBar(float adjustor)
    {
        plantSpawnBar.value += adjustor;
    }

    void ShakeBar(float speed, float amount)
    {
        if(speed == 0 && amount == 0)
        {
            return;
        }
        Vector3 tempVector = plantSpawnBar.transform.localPosition;
        tempVector.x = Mathf.Sin(Time.time * speed) * amount;
        tempVector.y = Mathf.Cos(Time.time * speed) * amount;
        float diff = startY + tempVector.y;
        tempVector.y = diff;
        plantSpawnBar.transform.localPosition = tempVector;
    }

    void DetermineShakeLevel()
    {
        if((plantSpawnBar.value / plantSpawnBar.maxValue * 100) < 60)
        {
            fillArea.color = Color.green;
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
            fillArea.color = Color.red;
            amount = 10.0f;
            speed = 20.0f;
        }
        if ((plantSpawnBar.value / plantSpawnBar.maxValue * 100) >= 90)
        {
            amount = 15.0f;
            speed = 25.0f;
        }
    }

    public void ActivateSpawner()
    {
        beginBarTimer = true;
        spawnEvent.Invoke();
        activateSpawnBar.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsAtMax())
        {
            amount = speed = 0f;
            activateSpawnBar.SetActive(true);
        }
        if (beginBarTimer)
        {
            if (Time.time > spawnTick)
            {
                spawnTick = Time.time + runtimeVal;
                AdjustBar(-0.1f);
                if(plantSpawnBar.value <= 0)
                {
                    beginBarTimer = false;
                }
            }
        }
        else
        {
            DetermineShakeLevel();
            ShakeBar(speed, amount);
        }
    }
}
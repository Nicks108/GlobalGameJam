using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
public class ResourceSystem : MonoBehaviour
{
    // TODO: Add goal (reach 50 days at 30% forest)
    // TODO: Name your company
    // TODO: Funny quote when you die or win
    // TODO: praise reward for high profits
    // TODO: Calculate profit gained from a day
    // TODO: Add rain visual effect for flood

    public float[] fundingVariables = new float[2] { 0f, 0f }; //values you need per day
    // 0 housing 1 meat 2 crops 3 default
    // H = profit
    // M = +profit, -food, more space
    // C = -profit, +food, less space
    public float[] percentFunded = new float[4] { 0f, 0f, 0f, 0f }; //live tick
    public float[] totalFunded = new float[3] { 0f, 0f, 0f };
    // population size dictates what you need to fund
    // e.g. 10 people = 1 house 10 food
    public float population;
    // Total money that acts as core resource
    // Flood drains profit, as does salaries?
    public float profit;
    public float temperature; // heat up the planet... maybe
    public float threatChance; // Percentage change of threat happening 
    public float forestPercentage;
    public int resourceIdx; // cycle through percentFunded
    bool isCoroutineGated;
    WaitForSeconds coroutineTimer = new WaitForSeconds(5.0f);
    bool isMoving; //debugging
    // Start is called before the first frame update
    void Start()
    {
        isMoving = false;
        isCoroutineGated = false;
        resourceIdx = 3; // default
    }

    public void SelectResource(string callback)
    {
        Debug.Log(callback);
        resourceIdx = callback switch
        {
            "1" => 0,
            "2" => 1,
            "3" => 2,
            "4" => 3,
            _ => resourceIdx = resourceIdx,
        };
    }

    public void GenerateQuestValues()
    {
        fundingVariables[0] = Mathf.Round(population / 10f);
        fundingVariables[1] = population;
    }

    public void IncrementFundedValue()
    {
        percentFunded[resourceIdx] += 0.1f;
        Debug.Log(percentFunded[resourceIdx]);
    }

    void CheckAction()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
        {
            InputSystem.onAnyButtonPress.CallOnce(ctrl => SelectResource(ctrl.name));
        }
    }

    void CheckMovement()
    {
        if(isMoving)
        {
            if (!isCoroutineGated)
            {
                isCoroutineGated = true;
                StartCoroutine(UpdateValues());
            }
        }
    }

    IEnumerator UpdateValues()
    {
        while (isMoving)
        {
            IncrementFundedValue();
            yield return coroutineTimer;
        }
        isCoroutineGated = false;
    }

    bool CheckThreat()
    {
        if(forestPercentage % 20 == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void UnleashThreat()
    {
        // play quick animation
        // reduce profits
        // calculate amount of land lost/saved:
        float destructionValue = Mathf.Abs(forestPercentage - 100);
        float tempProfit = QuickPercentCalc(ref profit, ref destructionValue);
        for (int i = 0; i < totalFunded.Length; i++)
        {
            float tempVal = QuickPercentCalc(ref totalFunded[i], ref destructionValue);
            totalFunded[i] = totalFunded[i] - tempVal;
        }
    }

    float QuickPercentCalc(ref float target, ref float percentage)
    {
        return (target / 100) * percentage;
    }

    void UpdateTemperature()
    {
        temperature = (Mathf.Abs(forestPercentage - 100))/10;
    }

    // Update is called once per frame
    void Update()
    {
        CheckAction();
        CheckMovement();
        if(CheckThreat()) { UnleashThreat(); }
        UpdateTemperature();
    }
}
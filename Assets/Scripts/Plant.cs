using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    Color[] randomColours = new Color[9] {Color.white,Color.black,Color.gray,
    Color.red,Color.green,Color.blue,Color.yellow,Color.magenta,Color.cyan};

    private const float waitSeconds = 1.0f;
    private const float StopSeconds = 5.0f;
    WaitForSeconds waitTime = new WaitForSeconds(waitSeconds);
    WaitForSeconds stopParticles = new WaitForSeconds(StopSeconds);
    ParticleSystem plantParticles;
    Vector3 veryFarAway = new Vector3(9999,9999,9999);
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void RandomiseColour()
    {
        GetComponent<Renderer>().material.color = randomColours[Random.Range(0,9)];
    }

    private void OnEnable()
    {
        if (plantParticles == null)
        {
            plantParticles = GetComponent<ParticleSystem>();
        }
        RandomiseColour();
        StartCoroutine(OverrideVolume());
        StartCoroutine(DisappearOverTime());
    }

    void OnDisable()
    {
        transform.position = veryFarAway;
    }

    IEnumerator DisappearOverTime()
    {
        yield return new WaitForSeconds(5.0f);
        gameObject.SetActive(false);
    }

    IEnumerator OverrideVolume()
    {
        yield return waitTime;
        GetComponent<S_VolumeObject>().Volume = 0.01f;
        yield return stopParticles;
        plantParticles.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

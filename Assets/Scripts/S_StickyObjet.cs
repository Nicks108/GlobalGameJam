using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;



[RequireComponent(typeof(AudioSource))]
public class S_StickyObjet : MonoBehaviour
{
    public GameObject HitParticals;
    public List<AudioClip> hitAudioClips = new List<AudioClip>();
    private AudioSource audioSource;

    public AnimationCurve HitReboundCurve;

    public int Score = 1;

    // Start is called before the first frame update
    void Start()
    {
        if (!HitParticals)
            HitParticals = LoadResorce("Hit_02") as GameObject;

        if (hitAudioClips.Count <=0)
        {
            hitAudioClips.Add(LoadResorce("Audio/Sound Effects/boing1") as AudioClip);
            hitAudioClips.Add(LoadResorce("Audio/Sound Effects/boing2") as AudioClip);

        }

        if (!audioSource)
            audioSource= GetComponent<AudioSource>();


        //Score = (int)(GetComponent<S_VolumeObject>().Volume * 10.0f);

        //HitParticals.transform.parent = this.transform;
        //HitParticals.transform.position = Vector3.zero;


    }

    // Update is called once per frame


    private bool isScaleInDirectionRunning = false;
    IEnumerator ScaleInDirection(Vector3 scaleDirectionVec)
    {
        isScaleInDirectionRunning = true;
        float endTime = 1;
        float currentTime = 0;
        Vector3 OriginalScale = this.transform.localScale;
        while (currentTime < endTime)
        {
            currentTime += Time.deltaTime;
            float Evalu = HitReboundCurve.Evaluate(currentTime);
            Vector3  newScale = Vector3.Scale(OriginalScale, scaleDirectionVec * Evalu);
            transform.localScale = OriginalScale+ newScale;
            yield return new WaitForEndOfFrame();
        }
        transform.localScale = OriginalScale;
        isScaleInDirectionRunning = false;

    }


    public void HitAnimation(Collision collision)
    {
        //play hit particals
        GameObject newHitParticals = Instantiate(HitParticals);
        newHitParticals.GetComponent<ParticleSystem>().Play();

        Vector3 HitPoint = collision.GetContact(0).point;

        newHitParticals.transform.position = HitPoint;

        audioSource.clip = hitAudioClips[Random.Range(0, hitAudioClips.Count)];
        audioSource.Play();


        Vector3 halfwayVec = (Vector3.down + (HitPoint - transform.position)).normalized;
        halfwayVec *= -1;
        Debug.DrawRay(transform.position, halfwayVec*10, Color.red,5);

        if(!isScaleInDirectionRunning)
            StartCoroutine(ScaleInDirection(halfwayVec));



        //stretch the model
    }

    private static UnityEngine.Object LoadResorce(string file)
    {
        var Resorce = Resources.Load(file);
        if (Resorce == null)
        {
            throw new FileNotFoundException("...no file found - please check the configuration");
        }

        return Resorce;
    }
}

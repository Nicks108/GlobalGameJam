using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class BlastVegetables : MonoBehaviour
{
    public UnityEvent scoreUpdate;
    public List<float> vegetableScoreValues;
    public List<GameObject> allVegetables;
    public GameObject vegetableTarget;
    public float movementSpeed;
    
    WaitForSeconds coroutineCD = new WaitForSeconds(0.08f);

    public float range = 1;

    // Start is called before the first frame update
    void Start()
    {
     
        movementSpeed = 25f;

        GameObject.Find("RangeProjetor").GetComponent<Projector>().orthographicSize = range;
    }

    public void PopulateVegetableArray(GameObject vegetable)
    {
        allVegetables.Add(vegetable);
    }

    

    IEnumerator FireAllVegetables()
    {
        S_PlayerControler.canMove = false;
        if(allVegetables.Count>0)
        {
            //transform.DetachChildren();
            GameObject tempObj = allVegetables[0];
            tempObj.transform.parent = null;
            StartCoroutine(MoveTowardsTarget(tempObj));
            allVegetables.Remove(tempObj);
            yield return coroutineCD; //yield return null;
        }
        else
        {
            //allVegetables.Clear();
            vegetableScoreValues.Clear();
        }
        S_PlayerControler.canMove = true;
    }

    IEnumerator MoveTowardsTarget(GameObject vegetable)
    {
        scoreUpdate.Invoke();
        while (Vector3.Distance(vegetable.transform.position, vegetableTarget.transform.position) > 0.5f)
        {
            vegetable.transform.position = Vector3.MoveTowards(vegetable.transform.position, vegetableTarget.transform.position, movementSpeed * Time.deltaTime);
            yield return null;
        }
        
        //RESPAWN VEGETABLES
    }

    // Update is called once per frame
    void Update()
    { 

    }

    void OnFire(InputValue MovmentValue)
    {
        if (Vector3.Distance(this.transform.position, vegetableTarget.transform.position) < range)
        {
            if(allVegetables.Count<=0)
                return;
            int score = allVegetables[0].GetComponent<S_StickyObjet>().Score;
            GameObject.Find("UI MANAGER").GetComponent<UIManager>().OnAddScore(score);

            //Vector3 Velocity = BallisticVelocity(this.transform.position, vegetableTarget.transform.position, Vector3.zero);
            //allVegetables[0].transform.parent = null;
            //allVegetables[0].AddComponent<Rigidbody>();
            //allVegetables[0].GetComponent<Rigidbody>().AddForce(Velocity.normalized + (Vector3.up*2));
            //allVegetables.Remove(allVegetables[0]);
            StartCoroutine(FireAllVegetables());

            
        }

    }


    float projectileSpeed = 100; // or whatever
    float accuracy = 10;
    Vector3 BallisticVelocity(Vector3 source, Vector3 target, Vector3 targetVelocity)
    {
        // use a few iterations of t$$anonymous$$s recursive function to zero in on 
        // where the target will be, when the projectile gets there
        Vector3 horiz = new Vector3(target.x - source.x, 0, target.z - source.z);
        float t = horiz.magnitude / projectileSpeed;
        for (int a = 0; a < accuracy; a++)
        {
            horiz = new Vector3(target.x + targetVelocity.x * t - source.x, 0, target.z + targetVelocity.z * t - source.z);
            t = horiz.magnitude / projectileSpeed;
        }
        // after t seconds, the cannonball will reach the horizontal location of the target -
        // so all we have to do is make sure its 'y' coordinate zeros out right there
        float gravityY = (.5f * Physics.gravity * t * t).y;
        // now we've calculated how much the projectile will fall during that time
        // so let's add a 'y' component to the velocity that will take care of the rest
        float yComponent = (target.y - source.y - gravityY) / t + targetVelocity.y;
        horiz = horiz.normalized * projectileSpeed;
        return new Vector3(horiz.x, yComponent, horiz.z);
    }

}

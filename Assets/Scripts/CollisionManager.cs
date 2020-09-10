using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CollisionManager : MonoBehaviour
{

#region SINGLETON PATTERN
 public static CollisionManager _instance;
 public static CollisionManager Instance
 {
     get {
         if (_instance == null)
         {
             _instance = GameObject.FindObjectOfType<CollisionManager>();
             
             if (_instance == null)
             {
                 GameObject container = new GameObject("CollisionManager");
                 _instance = container.AddComponent<CollisionManager>();
             }
         }
     
         return _instance;
     }
 }
 #endregion
    
    public Queue<Attractor> possibleAttractors = new Queue<Attractor>();
    [Tooltip("Ball Prefab needs Attractor script on it")]

    public GameObject ballPrefab;
    private bool[,] thisUpdateCalculated;
    private float timer=0, interval = 0.25f;
    
    public int numberOfBallsInSimulation;
    [Space]
    [Header("Dynamic Data")]
    public List<Attractor> activeAttractors = new List<Attractor>();
    public int counter =0;
    private TextMeshProUGUI counterText;
    private Vector3 screenEdgesMaxPosition, screenEdgesMinPosition;
    

    private void Awake() 
    {
        //initialize variables
        counterText = GameObject.FindObjectOfType<TextMeshProUGUI>();
        counterText.text = "0";
        //spawn of new object will occure only in a plane half max camera distance
        //and it is resolution independent.
        screenEdgesMaxPosition = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height, Camera.main.farClipPlane/2));
        screenEdgesMinPosition = Camera.main.ScreenToWorldPoint(new Vector3(0,0, Camera.main.farClipPlane/2));
        //initialize Object Pooling.
        PopulatePool();   
        
    }

    
    private void FixedUpdate() 
    {
        //until not all ball where spawned 
        if(counter < numberOfBallsInSimulation)
        {
        //balls will attract
        CalculateAttractions();
        //every interval add to counter and show it
        timer += Time.fixedDeltaTime;
    
            if(timer >= interval)
            {
                timer =0;
                counter++;
                counterText.text = counter.ToString();
                //get randomized position inside camera view
                Vector3 randPos = new Vector3(Random.Range(screenEdgesMinPosition.x, screenEdgesMaxPosition.x),Random.Range(screenEdgesMinPosition.y, screenEdgesMaxPosition.y),Random.Range(screenEdgesMinPosition.z,screenEdgesMaxPosition.z));
                //and spawn a ball form pool there if we have anything in there
                if(possibleAttractors.Count>1)SpawnFromPool(randPos);
                
            }
        }
        else
        //when we spawn all balls change from attraction to repultion
        {
            CalculateRepultions();
        }

        
    }
    private void CalculateAttractions()
    {
        //no need to calculete with less then 2 balls...
        if(activeAttractors.Count >=2)
        {
            //we need to have a flag that we allready calculated this pair
            thisUpdateCalculated = new bool[activeAttractors.Count, activeAttractors.Count];
            for (int i = 0; i < activeAttractors.Count; i++)
            {
                for (int j  = 0; j < activeAttractors.Count; j++)
                {
                    //not calculating with it self...
                    if(i==j)continue;
                    //if this pair was calculated allready go to next one
                    else if(thisUpdateCalculated[i,j])continue;
                    else
                    {
                        //calculate attraction and flag accordingly
                        activeAttractors[i]?.Attract(activeAttractors[j]);
                        thisUpdateCalculated[i,j] = true;
                        thisUpdateCalculated[j,i] = true;
                    }   
                }
                //when calculated all forces for this ball, applay all of them
                activeAttractors[i]?.ApplayForce();
            }
            
            
        }
    }
    private void CalculateRepultions()
    {
        //no need to calculete with less then 2 balls...
        if(activeAttractors.Count >=2)
        {
             //we need to have a flag that we allready calculated this pair
            thisUpdateCalculated = new bool[activeAttractors.Count, activeAttractors.Count];
            for (int i = 0; i < activeAttractors.Count; i++)
            {
                for (int j  = 0; j < activeAttractors.Count; j++)
                {
                    //not calculating with it self...
                    if(i==j)continue;
                    //if this pair was calculated allready go to next one
                    else if(thisUpdateCalculated[i,j])continue;
                    else
                    {
                        //calculate attraction and flag accordingly
                        activeAttractors[i]?.Repulse(activeAttractors[j]);
                        thisUpdateCalculated[i,j] = true;
                        thisUpdateCalculated[j,i] = true;
                    }   
                }
                //when calculated all forces for this ball, applay all of them
                activeAttractors[i]?.ApplayForce();
            }
        }
    }
    //initailizing pool
    private void  PopulatePool()
    {
        GameObject tempObj;
        for (int i = 0; i < numberOfBallsInSimulation; i++)
        {
            tempObj = Instantiate(ballPrefab);
            //just for convinience name then different then (clone) 
            tempObj.name = i.ToString();
            tempObj.SetActive(false);
        }
    }
    //this method is used at "bum" event..., it takes initial force vector as parameter
    public Attractor SpawnFromPool(Vector3 position, Vector3 initialForce)
    {
        Attractor tempAttractor = possibleAttractors.Dequeue();
        tempAttractor.gameObject.SetActive(true);
        tempAttractor.ResetToStartValues();
        tempAttractor.transform.position = position;
        tempAttractor.rb.AddForce(initialForce);
        return tempAttractor;
    }
    //Spawnes balls from pool with no initial force, it is used for initial spawning
    public void SpawnFromPool(Vector3 position)
    {
        GameObject tempAttractor = possibleAttractors.Dequeue().gameObject;
        tempAttractor.SetActive(true);
        tempAttractor.transform.position = position;

        
    }


}

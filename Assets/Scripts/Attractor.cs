using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Attractor : MonoBehaviour
{
   public Rigidbody rb;
   [SerializeField] private float startMass;
   [SerializeField] private float startDiameter = 1f;
   public float surface;
   //Yes, I know it is not needed to work, and it is not in scale, but still
   //You need to give credit to briliant mind that came up with it and use it 
   private const float G = 6.67f;
   [Space]
   [Header("Dynamic Data")]
   public float diameter = 1f;
   
   
   private bool colidable = true;
   public bool markToDisable = false;
   public List<Vector3> forces;
   
   //we can not disable and remove form activeAttractors if calculations are not 
   //finshed that is only reason for LateUpdate
   private void LateUpdate() 
   {
        if(markToDisable)
        {
            markToDisable = false;
            this.gameObject.SetActive(false);
        }

   } 
   //Calculate Attraction 
   public void Attract(Attractor toAttract)
   {
               
        Rigidbody rbToAttract = toAttract.rb;
        //calculate direction
        Vector3 direction = rbToAttract.position - rb.position;
        
        float distance = direction.magnitude;
        //and magnitude of attraction
        float forceMagnitude = ((rb.mass * rbToAttract.mass)/Mathf.Pow(distance, 2f))*G;       
        
        Vector3 force = direction.normalized * forceMagnitude;
        //add to list of all forces affecting this ball
        forces.Add(force);
        Vector3 opositeForce = force * -1f;
        //add to list of all forces affecting other ball
        toAttract.forces.Add(opositeForce);
 }
 //this way of collision detection is ok as long as we have not too many 
 //instances and CPU is not overwhelmed, I was trying to get colision from distance but 
 //it had almost the same result
   private void OnTriggerEnter(Collider other) 
   {
            //let only one of colliding balls execute collision calculation
            if( other.GetComponent<Attractor>()?.GetInstanceID() > GetInstanceID() )
            {
                //in scope of project collisions would end in merging only before
                // all balls where spawned
                if(CollisionManager.Instance.counter < CollisionManager.Instance.numberOfBallsInSimulation)
                Collide(other.GetComponent<Attractor>());
            }
           
   }
   private void Collide( Attractor collideWith)
   {
        if(colidable)
        {
            //add and calculate variables acordingly to project scope
            rb.mass += collideWith.rb.mass;

            surface += collideWith.surface;
            diameter = 2f * Mathf.Sqrt(surface / (Mathf.PI * 4f));
            this.gameObject.transform.localScale = new Vector3(diameter, diameter, diameter);
            //mark the other one to be disabled at the end of Update
            collideWith.markToDisable = true;
            //if mass exciedes 50 initial masses "Go BUM!"
            if(rb.mass >= 50 * startMass)StartCoroutine(Bum());
            
        }
   }
    private IEnumerator Bum()
    {
        //list is neeed to know with ones have collision disabled for 0.5s
        List<Attractor> bumList = new List<Attractor>();
        //calculate amout of basic balls that would be needet to create this one
        int ingreadientsAmount = Mathf.RoundToInt(rb.mass / startMass);
        //turn colision off for this one
        colidable = false;
        
        //Spawn all neeed balls
        for (int i = 0; i < ingreadientsAmount; i++)
        {
            //calculate random force 
            Vector3 randForce = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f)) * Random.Range(550f,5000f);
            //spawn only if there are suficient balls in pool
            if(CollisionManager.Instance.possibleAttractors.Count >0)
            {
                Attractor tempAttractor = CollisionManager.Instance.SpawnFromPool(rb.position, randForce);
                tempAttractor.colidable = false;
                tempAttractor.ResetToStartValues();
                //add spawned ball to list for future turning collision ON
                bumList.Add(tempAttractor);
            }
        } 
        //give this one collisions back and disable it
        colidable = true;     
        this.gameObject.SetActive(false);
        
        //wait 0.5s
        yield return new WaitForSeconds(0.5f);
        //enable collisions for all in list
        foreach (var item in bumList)
        {
            item.colidable = true;
        }
        
        
        yield return null;

    }
    //calculate vector sum of all forces affecting this ball and applay it to rigidbody
    public void ApplayForce()
    {
        if(forces.Count>0)
        {
        Vector3 forceSum = Vector3.zero;
        foreach (var item in forces)
        {   
            if(!float.IsNaN(item.x))
            forceSum += item;
        }

        if(!float.IsNaN(forceSum.x)) rb.AddForce(forceSum);
        //after forces where applayed claea their list
        forces.Clear();
        }
    }
    //oposite of Attraction, only difference is direction
    public void Repulse(Attractor toRepulse)
    {
        if(rb.mass >= 50 * startMass)StartCoroutine(Bum());
            
        Rigidbody rbToAttract = toRepulse.rb;
        //this is where the difference is
        Vector3 direction = rb.position - rbToAttract.position;
        
        float distance = direction.magnitude;
        
        float forceMagnitude = ((rb.mass * rbToAttract.mass)/Mathf.Pow(distance, 2f))*G;       
        
        Vector3 force = direction.normalized * forceMagnitude;
        forces.Add(force);
        Vector3 opositeForce = force * -1f;
        toRepulse.forces.Add(opositeForce);
        
   }
   public void ResetToStartValues()
   {
        diameter = startDiameter;
        rb.mass = startMass;
        surface = 4 * Mathf.PI * Mathf.Pow(diameter / 2f, 2);
        this.gameObject.transform.localScale = new Vector3(diameter, diameter, diameter);
        
   }
   private void OnEnable() 
   {
        CollisionManager.Instance.activeAttractors.Add(this);
        ResetToStartValues();
        
    }
   private void OnDisable() 
   {
        ResetToStartValues();
        CollisionManager.Instance.activeAttractors.Remove(this);
        CollisionManager.Instance.possibleAttractors.Enqueue(this);
   }


}

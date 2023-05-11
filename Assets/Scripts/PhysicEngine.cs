using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicEngine : MonoBehaviour
{
   public class BallNoCompute{
    public Vector3 position;
    public Vector3 force;
    public Vector3 velocity;
    public float mass;


    public GameObject ballObject;


}

    public float MaxForce =5f;

    public float MaxMass = 5f;
    

    public GameObject BallPrefab;

    
    public int HowManyBalls;
    public BoxCollider BoxCollider;
    
    private Vector3 Gravity = new Vector3(0f,-9.8f,0f);
    private Vector3 velocity;

    private HashSet<Vector3> positions = new HashSet<Vector3>();

    private BallNoCompute[] BallsArray;



    
    private void Start() {

        BallsArray = new BallNoCompute[HowManyBalls];
        
        for(int i=0; i< HowManyBalls;i++){
            Vector3 newPos = GetRandomPostion();

            BallNoCompute newball = new BallNoCompute();
            newball.position = newPos;
            newball.force = Vector3.zero;
            newball.velocity = Vector3.zero;
            newball.mass = GetRandomMass();
            GameObject ballInstance = Instantiate(BallPrefab,newPos,Quaternion.identity);
            newball.ballObject = ballInstance ;
            //newPos,GetRandomVelocity(),GetRandomMass(),ball);
            
            
            BallsArray[i] = newball;
        }
    }
    

 
    private void Update() { 
        //velocity.y -= Gravity * Time.deltaTime;
        //transform.position += velocity * Time.deltaTime;
        //CollisionDetector();
        //Dynamic Movement
        DynamicMovement();

//        Debug.Log(BoxCollider.bounds);
    }




    public void DynamicMovement(){

        for(int i=0; i<BallsArray.Length;i++){
            BallNoCompute current=BallsArray[i];
            //Force gravity = mass x gravity
            current.force +=  current.mass * Gravity;            
            // force/mass * time
            current.velocity += current.force/ current.mass * Time.deltaTime;
            current.position += current.velocity * Time.deltaTime;
            current.ballObject.transform.position = current.position;
            
            if(CheckWithinBounds(current)){
                //S=So + v*t
                current.position += current.velocity * Time.deltaTime;
                current.ballObject.transform.position = current.position;
            }else{
                current.velocity = -current.velocity ;
                current.position += current.velocity * Time.deltaTime;
                current.ballObject.transform.position = current.position;
            }
            //reset
            current.force = Vector3.zero;


        }
    }


    private Vector3 GetRandomPostion(){
        return new Vector3(
            Random.Range(BoxCollider.bounds.min.x,BoxCollider.bounds.max.x),
            Random.Range(BoxCollider.bounds.min.y,BoxCollider.bounds.max.y),
            Random.Range(BoxCollider.bounds.min.z,BoxCollider.bounds.max.z)
        );
    }


    private bool CheckWithinBounds(BallNoCompute ball){
        return BoxCollider.bounds.Contains(ball.position);
    }

    
    

    
    
    
    
    private float GetRandomMass(){
        return Random.Range(1,MaxMass);
    }

    private float GetRandomVelocity(){
        return Random.Range(5,MaxForce);
    }





 


}




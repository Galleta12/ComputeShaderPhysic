using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicCPUEngine : MonoBehaviour
{
   
    public class Ball{
        public Vector3 position;
        public Vector3 force;
        public Vector3 velocity;
        public float mass;
        public GameObject ballObject;

    };


    public GameObject BallPrefab;

     //varaible get from the manager
    public int ballsCount;

    public float maxMass;

    public BoxCollider BoxCollider;

 
    //array for the balls
    private Ball[] BallArray;
    //list of balls position gameobject
    //private List<GameObject> BallList = new List<GameObject>();
    
    private Vector3 Gravity = new Vector3(0f,-9.8f,0f);
    
    //drag or friction, reduce force when is colliding
    private Vector3 minBounds = new Vector3();
    private Vector3 maxBounds = new Vector3();


    private  void Awake()
    {
       
    }


    private void Start() {
        
       
        PopulateBallsArray();
       
    }



    private void PopulateBallsArray()
    {
        BallArray = new Ball[ballsCount];


       for (int i = 0; i < ballsCount; i++){
            Vector3 newPos = GetRandomPostion();

            Ball ballData = new Ball();
            ballData.position = newPos;
            ballData.force = Vector3.zero;
            ballData.velocity = Vector3.zero;
            ballData.mass = GetRandomMass();
            //instanciate the balls prefab
          
            GameObject newball = Instantiate(BallPrefab,newPos,Quaternion.identity);
            ballData.ballObject = newball;
            BallArray[i] = ballData;


       }

    }

  
    
    private void Update() {
        int interations = 20;
        float newDelatTime = Time.deltaTime/interations;

        for(int i=0; i <interations; i++){
            //generate values
            SimpleDynamics(newDelatTime);
            // collision detection
            MovementBall(newDelatTime);
            //move objects
        }
       //DynamicMovement();
        
    }

    

    // public void DynamicMovement(){

    //     for(int i=0; i<BallArray.Length;i++){
    //         Ball current=BallArray[i];
    //         //Force gravity = mass x gravity
    //         current.force +=  current.mass * Gravity;            
    //         // force/mass * time
    //         current.velocity += current.force/ current.mass * Time.deltaTime;
    //         current.position += current.velocity * Time.deltaTime;
    //         current.ballObject.transform.position = current.position;
            
    //         // if(CheckWithinBounds(current)){
    //         //     //S=So + v*t
    //         //     current.position += current.velocity * Time.deltaTime;
    //         //     current.ballObject.transform.position = current.position;
    //         // }else{
    //         //     current.velocity = -current.velocity;
    //         //     current.position += current.velocity * Time.deltaTime;
    //         //     current.ballObject.transform.position = current.position;
    //         // }
    //         // //reset
    //         current.force = Vector3.zero;


    //     }
    // }



    //first we are setting up the initial values
    //the initial forces will be gravity, if the object is already on  the floor
    //we can reduce the gravity
    private void SimpleDynamics(float deltaTime)
    {
        for(int i=0; i<BallArray.Length; i++){  
            
           
            Ball current = BallArray[i];
            //Force gravity = mass x gravity
            current.force +=  current.mass * Gravity;            
            // force/mass * time
            current.velocity += current.force/ current.mass * Time.deltaTime;
            //BallList[i].transform.position = current.position;
           // current.position += current.velocity * Time.deltaTime;
            if(!CheckWithinBounds(current)){
                
                current.velocity = -current.velocity;
                current.velocity *= 0.98f;
            }
            
            current.force *= 0.7f;
            BallArray[i] = current;
            // current.ballObject.transform.position = current.position;
            // // //current.velocity *= 0.98f;
            // current.force = Vector3.zero;

        }
    }


    private void MovementBall(float deltaTime){
        for(int i=0; i<BallArray.Length; i++){
            Ball current = BallArray[i];
            current.position += current.velocity * deltaTime;
            //reset force
            current.force = Vector3.zero;
            //change the position
            current.ballObject.transform.position = current.position;
            BallArray[i] = current;
        }
    }






    private bool CheckWithinBounds(Ball ball){
        return BoxCollider.bounds.Contains(ball.position);
    }

    private bool WithinBoundsY(Ball ball){

        return ball.position.y >=  minBounds.y || ball.position.y <= maxBounds.y;
    }

    private float GetRandomMass(){
        return Random.Range(1,maxMass);
    }

    private Vector3 GetRandomPostion(){
        return new Vector3(
            Random.Range(BoxCollider.bounds.min.x,BoxCollider.bounds.max.x),
            Random.Range(BoxCollider.bounds.min.y,BoxCollider.bounds.max.y),
            Random.Range(BoxCollider.bounds.min.z,BoxCollider.bounds.max.z)
        );
    }

}

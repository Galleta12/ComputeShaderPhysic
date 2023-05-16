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
  
    private Vector3 Gravity = new Vector3(0f,-9.8f,0f);
    public float radius = 0.15f;


    


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
            //scale the sphere
            newball.transform.localScale = Vector3.one * (radius* 2);
            ballData.ballObject = newball;
            BallArray[i] = ballData;


       }

    }

  
    
    private void Update() {
        int interations = 5;
        float newDelatTime = Time.deltaTime/interations;

        for(int i=0; i <interations; i++){
            //generate values
            SetInitialValues(newDelatTime);
            // collision detection
            Collisions(newDelatTime);
            //move objects
            MovementBall(newDelatTime);
        }
       //DynamicMovement();
        
    }

    
    
    //first we are setting up the initial values
    //the initial forces will be gravity
    private void SetInitialValues(float deltaTime){
        
        //start setting up the initial values, if it is colliding with the floor
        //we dont want to include force
        for(int i=0 ; i<BallArray.Length;i++){
            Ball current = BallArray[i];
             //Force gravity = mass x gravity
            current.force +=  current.mass * Gravity;            
            // force/mass * time
            current.velocity += current.force/ current.mass * Time.deltaTime;
            //decrement the velocty
            if(!CheckWithinBounds(current)){
                current.velocity = -current.velocity;
                
                // if(current.position.y <= minBounds.y ||
                // current.position.y >= maxBounds.y){
                current.velocity *= 0.98f;
                    
                //}
            }
            BallArray[i] = current;

        }
    }
    
    //collisions
    //collision in the wall
    private void Collisions(float deltaTime){
        for(int i=0; i <BallArray.Length-1; i++){
            //first handle conllision with wall
            Ball ball1 = BallArray[i];
            for(int j=i+1; j<BallArray.Length; j++){
                Ball ball2 = BallArray[j];
                CheckCollisionWitOtherBalls(ball1, ball2,i,j);
            }
          BallArray[i] = ball1;
        }
        
    }

    private void CheckCollisionWitOtherBalls(Ball ball1, Ball ball2, int i, int j)
    {
        Vector3 dist = ball1.position - ball2.position;
        float distanceLenght =  Vector3.Distance(ball1.position, ball2.position);
        //collision
        if(distanceLenght < 2 *radius){
            Vector3 normal = dist.normalized;
            Vector3 relativeVelocity = ball2.velocity - ball1.velocity;
            //impulse along the normal
            float impulse = Vector3.Dot(relativeVelocity,normal);
            ball1.velocity += impulse * normal;
            ball2.velocity -= impulse * normal;            
            //update the balls
            BallArray[j] = ball2;

        }
    }
    
   
 

    private void MovementBall(float deltaTime){
        for(int i=0; i<BallArray.Length; i++){
            Ball current = BallArray[i];
            current.position += current.velocity * deltaTime;
            //reset force
            //change the position
            
            current.force = Vector3.zero;
            current.ballObject.transform.position = current.position;
            
            BallArray[i] = current;
        }
    }






    private bool CheckWithinBounds(Ball ball){
        return BoxCollider.bounds.Contains(ball.position);
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

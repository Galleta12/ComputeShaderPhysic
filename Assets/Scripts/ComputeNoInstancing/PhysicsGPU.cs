using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsGPU : MonoBehaviour
{
   
    public struct Ball{
        public Vector3 position;
        public Vector3 force;
        public Vector3 velocity;
        
         public Color color;
        public float mass;


    };

    public GameObject BallPrefab;
    public ComputeShader computeShader;

  
    public int ballsCount = 5;
    public float maxMass = 5f;

    public BoxCollider BoxCollider;

      //radiuse of ball
    public float radius = 0.15f;


    private Ball[] BallArray;
    //save reference to the gameobjects in the world
    private List<GameObject> BallList = new List<GameObject>();

    private int kernelSetValues;
    private int kernelCollisions;
    private int kernelMoveBall;


    //buffer of the ball struct
    private ComputeBuffer ballsBuffer;

    //threads on the x block
    private int groupSizeX;


    // release the buffer when this script is no longer used.
    private void OnDestroy()
    {
        if (ballsBuffer != null)
        {
            ballsBuffer.Dispose();
        }
    }



    private void Start() {
        
        
        kernelSetValues = computeShader.FindKernel("SetInitialValues");
        kernelCollisions = computeShader.FindKernel("Collisions");
        kernelMoveBall = computeShader.FindKernel("MovementBall");

        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelSetValues, out x, out _, out _);
        //calculating how many threads we want luch per block
        //we add one so we make sure to dont get 0 threads.
        groupSizeX = ballsCount / (int)x + 1;
        
        
        PopulateBallsArray();
        StartShader();
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
            //set the colors
            ballData.color.r = Random.value;
            ballData.color.g = Random.value;
            ballData.color.b = Random.value;
            ballData.color.a = 1;

            BallArray[i] = ballData;
              //instanciate the balls prefab
          
            GameObject newball = Instantiate(BallPrefab,newPos,Quaternion.identity);
            //scale the sphere
            newball.transform.localScale = Vector3.one * (radius*2);

            BallList.Add(newball);


       }

    }


    private void StartShader()
    {
        // initialize ball buffer
        ballsBuffer = new ComputeBuffer(ballsCount, 14 * sizeof(float));
        ballsBuffer.SetData(BallArray);
        
        
        computeShader.SetBuffer(kernelSetValues, "ballsBuffer", ballsBuffer);
        
        computeShader.SetBuffer(kernelCollisions, "ballsBuffer", ballsBuffer);
        //populate the data and buffers in the compute shader for the dynamic
        computeShader.SetBuffer(kernelMoveBall, "ballsBuffer", ballsBuffer);



        computeShader.SetInt("ballsCount", ballsCount);
        computeShader.SetFloat("radius", radius);
        Vector3 minBounds = BoxCollider.bounds.min;
        Vector3 maxBounds = BoxCollider.bounds.max;
        computeShader.SetVector("minBounds", minBounds);
        computeShader.SetVector("maxBounds", maxBounds);

       
        
    }

    private void Update()
    {
        //this is for physics, the best is to calculate the update value several times per screen update
        //hence the ball move small amounts, hence the ball doesnt move fast 
        int iterations = 5;
        computeShader.SetFloat("deltaTime", Time.deltaTime/iterations);

        for (int i = 0; i < iterations; i++)
        {
            
            computeShader.Dispatch(kernelSetValues, groupSizeX, 1, 1);
            computeShader.Dispatch(kernelCollisions, groupSizeX, 1, 1);
            computeShader.Dispatch(kernelMoveBall, groupSizeX, 1, 1);
        }
        ballsBuffer.GetData(BallArray);
        for(int i=0; i<BallArray.Length;i++){
           BallList[i].transform.position = BallArray[i].position;
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

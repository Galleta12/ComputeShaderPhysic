using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPhysicsEngine : MonoBehaviour
{
    

    
    public struct Ball{
        public Vector3 position;
        public Vector3 force;
        public Vector3 velocity;
        public Color color;
        public float mass;

    

    };


    public ComputeShader computeShader;

    public Mesh ballMesh;

    public Material ballMaterial;


    public int ballsCount = 5;
    public float maxMass = 5f;

    public BoxCollider BoxCollider;

      //radiuse of ball
    public float radius = 0.15f;


    private Ball[] BallArray;

    private int kernelSetValues;
    private int kernelCollisions;
    private int kernelMoveBall;


    //buffer of the ball struct
    private ComputeBuffer ballsBuffer;
    //buffer for the surface shader
    private ComputeBuffer argsBuffer;
    //buffer from the DrawMeshInstancedIndirect
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    //threads on the x block
    private int groupSizeX;

    private Bounds bounds;
    
    //used to draw multiole objects with the same material
    private MaterialPropertyBlock props;




      // release the buffers when this script is no longer used.
    private void OnDestroy()
    {
        if (ballsBuffer != null)
        {
            ballsBuffer.Dispose();
        }

        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
        }
    }
    
    
    

    
    
    
    
    private void Start() {
        
        
        //kernel for the initial forces
         kernelSetValues = computeShader.FindKernel("SetInitialValues");
        //kernel for the collisions 
        kernelCollisions = computeShader.FindKernel("Collisions");
        //kernel function for the dynamic (move the balls)
        kernelMoveBall = computeShader.FindKernel("MovementBall");
        //getting the threads in the x axis
        //data varaible store no negative variables
        //all of the functions has the same groups threads hence we can just the the threds on the x component
        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelSetValues, out x, out _, out _);
        //calculating how many threads we want luch per block
        //we add one so we make sure to dont get 0 threads.
        groupSizeX = ballsCount / (int)x + 1;


        PopulateBallsArray();
        StartShader();
        SurfaceShaderBuffers();
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
            ballData.color.r = Random.value;
            ballData.color.g = Random.value;
            ballData.color.b = Random.value;
            ballData.color.a = 1;
            
            BallArray[i] = ballData;

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



    private void SurfaceShaderBuffers(){

        //setting up the material prop, assinging the unique id of this material
        props = new MaterialPropertyBlock();
        props.SetFloat("_UniqueID", Random.value);
        //bounds vectors, bounds of the materials
        bounds = new Bounds(Vector3.zero, Vector3.one * radius);
       
        //for the shader buffer
        //they need to have 5 arguments, this will be passed to the surface shader(vertex shader)

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (ballMesh != null)
        {
            //vertex, number of traigle indices
            args[0] = (uint)ballMesh.GetIndexCount(0);
            //number of balls
            args[1] = (uint)ballsCount;
            // //this are for submeshes, we are not using submeshes on this project
            // args[2] = ;
            // args[3] = ;
        }
        argsBuffer.SetData(args);

        //for the surface shader
        ballMaterial.SetFloat("_Radius", radius*2);
        //pass the balls ballsbuffer to the ballsbuffer in the surfaceshader
        //this is not a read write buffer, since those things are done by the compute shader
        ballMaterial.SetBuffer("ballsBuffer", ballsBuffer);

    }


    private void Update()
    {
        //this is for physics, the best is to calculate the update values several times per screen update
        //hence the ball move small amounts, hence the ball doesnt move fast 
        int iterations = 5;
        computeShader.SetFloat("deltaTime", Time.deltaTime/iterations);

        for (int i = 0; i < iterations; i++)
        {
            
            computeShader.Dispatch(kernelSetValues, groupSizeX, 1, 1);
            computeShader.Dispatch( kernelCollisions, groupSizeX, 1, 1);
            computeShader.Dispatch( kernelMoveBall, groupSizeX, 1, 1);
        }

        GraphicDrawInstance();
       
    }


    private void GraphicDrawInstance(){
        //draw the surface shader, passin our ball materia and buffer
        //bounds is the volume of the instance of the ball
        //props is the material properties
        //This is how to draw instances of the same mesh, which will make the GPU to handle all the renderings
        //We dont need to upload or update the instances ever frame, with this approach Unity, the data is stored indefinitely in the GPU
        // This will make it easier to use a compute shader share buffer
        Graphics.DrawMeshInstancedIndirect(ballMesh, 0, ballMaterial, bounds, argsBuffer, 0, props);
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

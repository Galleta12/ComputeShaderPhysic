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

    public int ballsCount = 5;
    public float maxMass = 5f;
    
    public BoxCollider BoxCollider;
    public ComputeShader computeShader;

    public Mesh ballMesh;

    public Material ballMaterial;

    //radiuse of ball
    public float radius = 0.15f;

    private Ball[] BallArray;

    private int kernelDynamicValues;
    private int kernelCollisions;
    private int kernelCollisionsReponse;


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
        
        
        //kernel for the collision
        kernelCollisions = computeShader.FindKernel("CollisionDetection");
        //kernel for the collisions reponse
        kernelCollisions = computeShader.FindKernel("CollisionReponse");        
        //kernel function for the dynamic (move the balls)
        kernelDynamicValues = computeShader.FindKernel("DyncamicValues");
        //getting the threads in the x axis
        //data varaible store no negative variables
        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelDynamicValues, out x, out _, out _);
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
        //populate the data and buffers in the compute shader
        computeShader.SetBuffer(kernelDynamicValues, "ballsBuffer", ballsBuffer);
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
        //bounds vectors, 
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
       
        //for the shader buffer
        //they need to have 5 arguments, this will be passed to the surface shader(vertex shader)

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (ballMesh != null)
        {
            //vertex, number of traigle indices
            args[0] = (uint)ballMesh.GetIndexCount(0);
            //number of balls
            args[1] = (uint)ballsCount;
            //this are for submeshes, we are not using submeshes on this project
            args[2] = (uint)ballMesh.GetIndexStart(0);
            args[3] = (uint)ballMesh.GetBaseVertex(0);
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
        //this is for physics, the best is to calculate the update value several times per screen update
        //hence the ball move small amounts, hence the ball doesnt move fast 
        int iterations = 5;
        computeShader.SetFloat("deltaTime", Time.deltaTime/iterations);

        for (int i = 0; i < iterations; i++)
        {
            
            computeShader.Dispatch(kernelCollisions, groupSizeX, 1, 1);
            computeShader.Dispatch(kernelCollisionsReponse, groupSizeX, 1, 1);
            computeShader.Dispatch(kernelDynamicValues, groupSizeX, 1, 1);
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



This is a project of a simple physics engine.

There are three different implementations, the first one is running the physic in the CPU.

The second one, uses a compute shader.

The third one uses a compute shader and a surface shader. Hence the rendering and the physics will be handled by the GPU.


This project was done in the Unity Version Unity Beta Version 2022.2.0b5. You can clone this repository and open it with this Unity
version 2022.2.0b5.

In the folder Scenes are the scenes of each implementation and the scripts are on the Scripts folder. All of these folders are inside
the Assets folder. You can also change the settings of each scrip in the inspector of the Unity Editor.
CPU implementation in the CPUSCENE. The script that runs this implementation is PhysisCPUEngine.cs 
Compute Shader implementation in the GPUBADSCENE. The scripts are PhysicsGPU.cs and BallGpuPhysics.compute
Compute Shader and Surface Shader in the GPUSCENE. BallPhysicsEngine.cs and  BallGpuPhysics.compute


![compute shader](https://github.com/Galleta12/ComputeShaderPhysic/assets/79543944/9d2da8f5-5ce0-4066-89b1-49a61af53d45)

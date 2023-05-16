Shader "Physics/BallShader" { 

   Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

	
	}
    //surface shader
   SubShader {
      

		CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #pragma instancing_options procedural:setup
        #pragma surface surf Standard vertex:vert addshadow fullforwardshadows        
        
		sampler2D _MainTex;
        //varaible for the surface
		half _Glossiness;
        half _Metallic;
        fixed4 _Color;
 
        //input for the surface shader,
        //worldPos, position of current pixel
        //this is automatically populated by Unity-built in vertex shader
		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
		};
	
    
        float3 _BallPosition;
        float _Radius;
        //poplutate ths buffer
         #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct Ball
            {
                float3 position;
                float3 force;
                float3 velocity;
                float4 color;
                float mass;
            };

            StructuredBuffer<Ball> ballsBuffer; 
         #endif

 
        //once per vertex of the mesh
         void vert(inout appdata_full v, out Input data)
        {
            UNITY_INITIALIZE_OUTPUT(Input, data);
            //move the balls 
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                //scale ball
                v.vertex.xyz *= _Radius;
                v.vertex.xyz += _BallPosition;
            #endif
        }
        //once per instance
        void setup()
        {
            //populate the varaibles from the buffer
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                //color value and position
                _Color = ballsBuffer[unity_InstanceID].color;
                _BallPosition = ballsBuffer[unity_InstanceID].position;
            #endif
        }
 
        //out put the final surface
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
		
         }
 
         ENDCG
    }
    FallBack "Diffuse"
    
}
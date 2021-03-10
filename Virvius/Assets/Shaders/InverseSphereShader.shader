 Shader "Custom/InverseSphereShader" {
     Properties
     {
         _Blend("Blend", Range(0, 1)) = 0.5
         _MainTex("Frist texture", 2D) = "black" { }
         _BlendTex("Second texture", 2D) = "black" { }
     }
     Category
     {
         Lighting Off
         ZWrite Off
         Cull Front
         Blend SrcAlpha OneMinusSrcAlpha
         SubShader
         {
             Pass
             {
 
                 SetTexture[_MainTex]
 
                 SetTexture[_BlendTex]
                 {
                     constantColor(0, 0, 0, [_Blend])
                     combine texture lerp(constant) previous
                 }
             }
         }
     }
 }
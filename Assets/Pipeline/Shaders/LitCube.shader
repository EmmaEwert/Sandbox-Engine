Shader "Pipeline/Lit Cube" {
	Properties {
		_Color     ("Color", Color) = (1, 1, 1, 1)
		_MainTex   ("Front", 2D) = "white" { }
		_BackTex   ("Back", 2D) = "white" { }
		_LeftTex   ("Left", 2D) = "white" { }
		_RightTex  ("Right", 2D) = "white" { }
		_TopTex    ("Top", 2D) = "white" { }
		_BottomTex ("Bottom", 2D) = "white" { }
	}
	SubShader {
		Pass {
			HLSLPROGRAM

			#pragma target 3.5

			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling

			#pragma vertex LitPassVertex
			#pragma fragment LitPassFragment

			#include "../ShaderLibrary/LitCube.hlsl"

			ENDHLSL
		}
	}
}
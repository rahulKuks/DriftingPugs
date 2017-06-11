// Copyright 2017 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Brush/Particle/Smoke" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_ScrollRate("Scroll Rate", Float) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
	Blend SrcAlpha One 
	AlphaTest Greater .01
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }

	SubShader {
		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles
			#pragma target 3.0
			#pragma multi_compile __ TBT_LINEAR_TARGET

			#include "UnityCG.cginc"
			#include "../../../Shaders/Include/Brush.cginc"
			#include "../../../Shaders/Include/Particles.cginc"
			#include "Assets/ThirdParty/Noise/Shaders/Noise.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			float4 _MainTex_ST;
			float _ScrollRate;

			//
			// Functions for line/plane distances. Experimental.
			//
			float dist_from_line(float3 line_dir, float3 point_on_line, float3 pos) {
				float3 point_to_line = pos - point_on_line;
				float3 dist_along_line = dot(point_to_line, line_dir);
				float3 closest_point_on_line = dist_along_line * line_dir + point_on_line;
				return length(closest_point_on_line - pos);
			}

			float dist_from_line_repeating(float3 line_dir, float3 point_on_line, float3 pos) {
				float3 point_to_line = pos - point_on_line;
				float3 dist_along_line = dot(point_to_line, line_dir);
				float3 closest_point_on_line = dist_along_line * line_dir + point_on_line;
				return length( sin(closest_point_on_line - pos));
			}

			float4 dist_from_plane(float3 plane_normal, float3 point_on_plane, float3 pos) {
				float dist = dot(plane_normal, pos - point_on_plane);
				float3 closest_point_on_plane = pos - dist * plane_normal;
				return float4(closest_point_on_plane.xyz, abs(dist));
			}

			v2f vert (ParticleVertex_t v)
			{
				v.color = TbVertToSrgb(v.color);
				v2f o;
				float4 worldPos = OrientParticle_WS(
						v.vid, v.corner.xyz, v.center,
						v.texcoord.z /* rotation */, v.texcoord.w /* birthTime */);

				// Compute Curl noise in a transform invariant space
				// TODO(pld): object/canvas space is more invariant than scene space,
				// so use OrientParticle rather than OrientParticle_WS
					worldPos = mul(xf_I_CS, worldPos);

				float t = _Time.y*_ScrollRate + v.color.a * 10;

				float time = _Time.x * 5;
				float d = 30;
				float freq = .1;
				float3 disp = float3(1,0,0) * curlX(worldPos.xyz * freq + time, d); 
				disp += float3(0,1,0) * curlY(worldPos.xyz * freq +time, d); 
				disp += float3(0,0,1) * curlZ(worldPos.xyz * freq + time, d);

				worldPos.xyz += disp * 5 * kDecimetersToWorldUnits;

				// Back to transformed canvas space
				worldPos = mul(xf_CS, worldPos);

				o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				o.worldPos = worldPos;

				o.color = v.color;
				v.color.a = 1;
				o.texcoord = TRANSFORM_TEX(v.texcoord.xy,_MainTex);

				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 worldPos = i.worldPos.xyz;
				float4 c =  tex2D(_MainTex, i.texcoord);
				c *= i.color * _TintColor;
				c = SrgbToNative(c);
				return c;
			}
			ENDCG 
		}
	}
}
}

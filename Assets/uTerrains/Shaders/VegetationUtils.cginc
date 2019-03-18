// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

//
// Taken from Unity's terrain engine shader helpers.
// This is needed because Unity's terrain engine shader helpers are 
// included in deployment only if the terrain engine is used. We need
// it to be independent from Unity's terrains.

// grass
fixed4 _WavingTint;
float4 _WaveAndDistance;	// wind speed, wave size, wind amount, max sqr distance
float _MaxSqrDistanceInverse;		// 1 / (max sqr distance)
float3 _CameraRight, _CameraUp;

// ---- Grass helpers

// Calculate a 4 fast sine-cosine pairs
// val: 	the 4 input values - each must be in the range (0 to 1)
// s:		The sine of each of the 4 values
// c:		The cosine of each of the 4 values
void FastSinCos (float4 val, out float4 s, out float4 c) {
	val = val * 6.408849 - 3.1415927;
	// powers for taylor series
	float4 r5 = val * val;					// wavevec ^ 2
	float4 r6 = r5 * r5;						// wavevec ^ 4;
	float4 r7 = r6 * r5;						// wavevec ^ 6;
	float4 r8 = r6 * r5;						// wavevec ^ 8;

	float4 r1 = r5 * val;					// wavevec ^ 3
	float4 r2 = r1 * r5;						// wavevec ^ 5;
	float4 r3 = r2 * r5;						// wavevec ^ 7;

	//Vectors for taylor's series expansion of sin and cos
	float4 sin7 = {1, -0.16161616, 0.0083333, -0.00019841};
	float4 cos8  = {-0.5, 0.041666666, -0.0013888889, 0.000024801587};

	// sin
	s =  val + r1 * sin7.y + r2 * sin7.z + r3 * sin7.w;

	// cos
	c = 1 + r5 * cos8.x + r6 * cos8.y + r7 * cos8.z + r8 * cos8.w;
}

fixed4 WaveGrass (inout float4 vertex, float waveAmount, fixed4 color)
{
	float4 _waveXSize = float4(0.012, 0.02, 0.06, 0.024) * _WaveAndDistance.y;
	float4 _waveZSize = float4 (0.006, .02, 0.02, 0.05) * _WaveAndDistance.y;
	float4 waveSpeed = float4 (0.3, .5, .4, 1.2) * 4;

	float4 _waveXmove = float4(0.012, 0.02, -0.06, 0.048) * 2;
	float4 _waveZmove = float4 (0.006, .02, -0.02, 0.1);

	float4 waves;
	waves = vertex.x * _waveXSize;
	waves += vertex.z * _waveZSize;

	// Add in time to model them over time
	waves += (_WaveAndDistance.x * _Time.x) * waveSpeed;

	float4 s, c;
	waves = frac (waves);
	FastSinCos (waves, s,c);

	s = s * s;
	
	s = s * s;

	float lighting = dot (s, normalize (float4 (1,1,.4,.2))) * .7;

	s = s * waveAmount;

	float3 waveMove = float3 (0,0,0);
	waveMove.x = dot (s, _waveXmove);
	waveMove.z = dot (s, _waveZmove);

	vertex.xz -= waveMove.xz * _WaveAndDistance.z;
	
	// apply color animation
	fixed3 waveColor = lerp (fixed3(0.5,0.5,0.5), _WavingTint.rgb, lighting);
	
	// Fade the grass out before detail distance.
	// Saturate because Radeon HD drivers on OS X 10.4.10 don't saturate vertex colors properly.
	float4 worldPos = mul(unity_ObjectToWorld, vertex);
	float3 offset = worldPos.xyz - _WorldSpaceCameraPos.xyz;
	color.a = saturate (2 * (_WaveAndDistance.w - dot (offset, offset)) * _MaxSqrDistanceInverse);
	
	return fixed4(2 * waveColor * color.rgb, color.a);
}

void BillboardGrass( inout float4 pos, float2 offset )
{
	float3 grasspos = pos.xyz - _WorldSpaceCameraPos.xyz;
	if (dot(grasspos, grasspos) > _WaveAndDistance.w)
		offset = 0.0;
    pos.xyz += offset.x * _CameraRight.xyz;
	pos.xyz += offset.y * _CameraUp.xyz;
}

void BillboardTree( inout float4 pos, float2 offset )
{
	pos.xyz += offset.x * _CameraRight.xyz;
	pos.xyz += offset.y * _CameraUp.xyz;
}

// Grass: appdata_full usage
// color		- .xyz = color, .w = wave scale
// normal		- normal
// tangent.xy	- billboard extrusion
// texcoord		- UV coords
// texcoord1	- 2nd UV coords

void WavingGrassVert (inout appdata_full v)
{
	// MeshGrass v.color.a: 1 on top vertices, 0 on bottom vertices
	// _WaveAndDistance.z == 0 for MeshLit
	float waveAmount = v.color.a * _WaveAndDistance.z;
	v.color = WaveGrass (v.vertex, waveAmount, v.color);
}

void WavingGrassBillboardVert (inout appdata_full v)
{
	BillboardGrass (v.vertex, v.tangent.xy);
	// wave amount defined by the grass height
	float waveAmount = v.tangent.y;
	v.color = WaveGrass (v.vertex, waveAmount, v.color);
}

void TreeBillboardVert (inout appdata_full v)
{
	BillboardTree (v.vertex, v.tangent.xy);
}

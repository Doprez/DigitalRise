//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialPom.fx
/// Combines the material of a model (e.g. textures) with the light buffer data.
/// Uses parallax occlusion mapping with soft self-shadows.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRise.Graphics/EffectsSource/Common.fxh"
#include "../../../Source/DigitalRise.Graphics/EffectsSource/Encoding.fxh"
#include "../../../Source/DigitalRise.Graphics/EffectsSource/Deferred.fxh"
#include "../../../Source/DigitalRise.Graphics/EffectsSource/Material.fxh"
#include "Parallax.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;
float3 CameraPosition : CAMERAPOSITION;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB)

// ----- Parallax Occlusion Mapping
float HeightScale = 0.03;
float HeightBias = 0;
float2 HeightTextureSize = float2(512, 512);
texture HeightTexture;
sampler HeightSampler = sampler_state
{
  Texture = <HeightTexture>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = Wrap;
  AddressV = Wrap;
};

// The mip level id for transitioning between the full POM computation and normal mapping.
int LodThreshold = 4;

// The minimum number of samples for sampling the height field profile
int MinSamples = 4;

// The maximum number of samples for sampling the height field profile
int MaxSamples = 20;

// For soft self-shadowing:
// A factor which defines the sampling distance.
float ShadowScale = 0.5;

// The number of samples for shadow computation.
int ShadowSamples = 4;

// The factor that reduces the influence of distant samples.
float ShadowFalloff = 0.33;

// A factor which makes shadows darker.
float ShadowStrength = 100;

float3 DirectionalLightDirection : DIRECTIONALLIGHTDIRECTION;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
  float3 Normal	: NORMAL;
  float3 Tangent : TANGENT;
  float3 Binormal : BINORMAL;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float3 PositionWorld : TEXCOORD2;
  float3 ViewDirectionTangent : TEXCOORD3; // View direction in tangent space.
  float3 LightDirectionTangent : TEXCOORD4;  // LightDirection in tangent space.
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float3 PositionWorld : TEXCOORD2;
  float3 ViewDirectionTangent : TEXCOORD3;
  float3 LightDirectionTangent : TEXCOORD4;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float4 instanceColorAndAlpha)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;

  float4 positionWorld = mul(input.Position, world);
  float4 positionView = mul(positionWorld, View);
  output.PositionWorld = positionWorld.xyz;
  output.Position = mul(positionView, Projection);
  output.PositionProj = output.Position;
  
  float3 normal =  mul(input.Normal, (float3x3)world);
  float3 tangent = mul(input.Tangent, (float3x3)world);
  float3 binormal = mul(input.Binormal, (float3x3)world);
  
  // Matrix that transforms column(!) vectors from world space to tangent space.
  float3x3 worldToTangent = float3x3(tangent, binormal, normal);
  
  float3 viewDirectionWorld = output.PositionWorld - CameraPosition;
  output.ViewDirectionTangent = mul(worldToTangent, viewDirectionWorld);
  output.LightDirectionTangent = mul(worldToTangent, DirectionalLightDirection);
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World, 0);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);

  return VS(input, world, colorAndAlpha);
}


float4 PS(PSInput input) : COLOR0
{
  float3 viewDirectionTangent = normalize(input.ViewDirectionTangent);
  float3 lightDirectionTangent = normalize(input.LightDirectionTangent);
  
  // Parallax occlusion mapping
  float mipLevel = MipLevel(input.TexCoord, HeightTextureSize);
  float4 pom = ParallaxOcclusionMapping(input.TexCoord.xy, HeightSampler,
    viewDirectionTangent, HeightScale, HeightBias, mipLevel, LodThreshold, MinSamples, MaxSamples,
    lightDirectionTangent, ShadowScale, ShadowSamples, ShadowFalloff, ShadowStrength);
  
  // Apply parallax to texture coordintates.
  float2 texCoord = pom.xy;
  
  float4 diffuseMap = tex2D(DiffuseSampler, texCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float4 specularMap = tex2D(SpecularSampler, texCoord);
  float3 specular = FromGamma(specularMap.rgb);
  
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  float4 color = float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 1);
  
  // Apply soft self-shadow.
  float shadow = pom.w;
  color.rgb *= (1 - shadow);
    
  return color;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX           // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
#if !SM4
    VertexShader = compile vs_3_0 VSNoInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSNoInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

technique DefaultInstancing
{
  pass
  {
#if !SM4
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

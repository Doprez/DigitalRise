﻿#if !ANDROID && !LINUX && !MACOS && !IOS    // saucer model uses .dds cube map which is not supported in OpenGL.
using DigitalRise.Geometry;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using DigitalRise.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using AssetManagementBase;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample shows how to use the XNA EnvironmentMapEffect.",
    "",
    23)]
  public class EnvironmentMapEffectSample : BasicSample
  {
    public EnvironmentMapEffectSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      SampleFramework.IsMouseVisible = false;
      GraphicsScreen.ClearBackground = true;
      GraphicsScreen.BackgroundColor = Color.CornflowerBlue;
      SetCamera(new Vector3(8, 6, 8), ConstantsF.PiOver4, -0.4f);

      // Load the saucer model. This model is processed using the DigitalRise Model 
      // Processor - not the default XNA model processor!
      // In the folder that contains saucer.fbx, there are several XML files (*.drmdl and *.drmat) 
      // which define the materials of the model. These material description files are 
      // automatically processed by the DigitalRise Model Processor. Please browse 
      // to the content folder and have a look at the *.drmdl and *.drmat files.
      var model = AssetManager.LoadDRModel(GraphicsService, "EnvironmentMapped/saucer.drmdl");

      // The XNA ContentManager manages a single instance of each model. We clone 
      // the models, to get a copy that we can modify without changing the original 
      // instance. 
      model = model.Clone();

      // Position the model and add it to the scene.
      model.PoseWorld = new Pose(RandomHelper.Random.NextQuaternion());
      model.ScaleLocal = new Vector3(0.4f);
      GraphicsScreen.Scene.Children.Add(model);

      //// Here is another example showing how you can change material properties 
      //// of the model: 
      //// Get the mesh (the saucer model contains only a single mesh node).
      //MeshNode meshNode = _model.FindFirstMeshNode();
      //Mesh mesh = meshNode.Mesh;
      //// Get the second material (which is the material of the cockpit).
      //Material material = mesh.Materials[1];
      //// Get the effect used in the "Default" render pass.
      //EffectBinding effectBindings = material["Default"];
      //// Change some parameters.
      //effectBindings.Set("EnvironmentMapAmount", 1f);
      //effectBindings.Set("FresnelFactor", 0.5f);
    }
  }
}
#endif
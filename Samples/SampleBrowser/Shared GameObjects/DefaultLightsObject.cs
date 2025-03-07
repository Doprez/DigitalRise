﻿using DigitalRise;
using DigitalRise.GameBase;
using DigitalRise.Geometry;
using DigitalRise.Graphics;
using DigitalRise.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using System;
using MathHelper = DigitalRise.Mathematics.MathHelper;

namespace Samples
{
	// Creates light sources with the same settings as BasicEffect.EnableDefaultLighting() 
	// in the XNA Framework.
	public class DefaultLightsObject : GameObject
	{
		private readonly IServiceProvider _services;
		private LightNode _ambientLightNode;
		private LightNode _keyLightNode;
		private LightNode _fillLightNode;
		private LightNode _backLightNode;


		public DefaultLightsObject(IServiceProvider services)
		{
			_services = services;
			Name = "DefaultLights";
		}


		// OnLoad() is called when the GameObject is added to the IGameObjectService.
		protected override void OnLoad()
		{
			var ambientLight = new AmbientLight
			{
				//Color = new Vector3(0.05333332f, 0.09882354f, 0.1819608f),  // XNA BasicEffect Values
				Color = new Vector3(0.5f),                                    // Make ambient light brighter.
				Intensity = 1,
				HemisphericAttenuation = 1,
			};
			_ambientLightNode = new LightNode(ambientLight);

			var keyLight = new DirectionalLight
			{
				Color = new Vector3(1, 0.9607844f, 0.8078432f),
				DiffuseIntensity = 1,
				SpecularIntensity = 1,
			};
			_keyLightNode = new LightNode(keyLight)
			{
				Name = "KeyLight",
				Priority = 10,   // This is the most important light.
				PoseWorld = new Pose(MathHelper.CreateRotation(Vector3.Forward, new Vector3(-0.5265408f, -0.5735765f, -0.6275069f))),
			};

			var fillLight = new DirectionalLight
			{
				Color = new Vector3(0.9647059f, 0.7607844f, 0.4078432f),
				DiffuseIntensity = 1,
				SpecularIntensity = 0,
			};
			_fillLightNode = new LightNode(fillLight)
			{
				Name = "FillLight",
				PoseWorld = new Pose(MathHelper.CreateRotation(Vector3.Forward, new Vector3(0.7198464f, 0.3420201f, 0.6040227f))),
			};

			var backLight = new DirectionalLight
			{
				Color = new Vector3(0.3231373f, 0.3607844f, 0.3937255f),
				DiffuseIntensity = 1,
				SpecularIntensity = 1,
			};
			_backLightNode = new LightNode(backLight)
			{
				Name = "BackLight",
				PoseWorld = new Pose(MathHelper.CreateRotation(Vector3.Forward, new Vector3(0.4545195f, -0.7660444f, 0.4545195f))),
			};

			var scene = _services.GetService<IScene>();
			scene.Children.Add(_ambientLightNode);
			scene.Children.Add(_keyLightNode);
			scene.Children.Add(_fillLightNode);
			scene.Children.Add(_backLightNode);
		}


		// OnUnload() is called when the GameObject is removed from the IGameObjectService.
		protected override void OnUnload()
		{
			var scene = _ambientLightNode.Parent;

			scene.Children.Remove(_ambientLightNode);
			_ambientLightNode.Dispose(false);
			_ambientLightNode = null;

			scene.Children.Remove(_keyLightNode);
			_keyLightNode.Dispose(false);
			_keyLightNode = null;

			scene.Children.Remove(_fillLightNode);
			_fillLightNode.Dispose(false);
			_fillLightNode = null;

			scene.Children.Remove(_backLightNode);
			_backLightNode.Dispose(false);
			_backLightNode = null;
		}
	}
}

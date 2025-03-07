﻿using DigitalRise.Animation;
using DigitalRise.Input;
using DigitalRise.UI;
using Microsoft.Xna.Framework;
using AssetManagementBase;
using System;
using Microsoft.Xna.Framework.Graphics;
using DigitalRise;
using System.ComponentModel.Design;

namespace Samples
{
  // Samples in this solution are derived from the XNA class GameComponent. The
  // abstract class Sample can be used as the base class of samples. It provides
  // access to the game services (input, graphics, physics, etc.).
  // In addition, it creates a new ServiceContainer which can be used in samples.
  //
  // ----- Clean up:
  // When this class is disposed, it performs common clean-up operations to restore 
  // a clean state for the next sample instance. Things that are automatically removed:
  // - GameObjects
  // - GraphicsScreens
  // - Physics objects
  // - ParticleSystems
  // Other objects have to be cleaned up manually (e.g. UIScreens, Animations, etc.)!
  public class Sample: IDisposable
  {
    // Services which can be used in derived classes.
    protected readonly GraphicsDevice GraphicsDevice;
    protected readonly ServiceContainer Services;
    protected readonly AssetManager AssetManager;
    protected readonly IInputService InputService;
    protected readonly IAnimationService AnimationService;
    protected readonly IUIService UIService;
    protected Color BackgroundColor = Color.Black;


    protected Sample()
    {
      // Get services from the global service container.
      Services = new ServiceContainer(SampleGame.Instance.Services);
      AssetManager = Services.GetService<AssetManager>();
      InputService = Services.GetService<IInputService>();
      AnimationService = Services.GetService<IAnimationService>();
      UIService = Services.GetService<IUIService>();
      GraphicsDevice = Services.GetService<GraphicsDevice>();
    }

		~Sample()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Render(GameTime gameTime)
    {
			// Clear background.
			GraphicsDevice.Clear(BackgroundColor);
		}
	}
}

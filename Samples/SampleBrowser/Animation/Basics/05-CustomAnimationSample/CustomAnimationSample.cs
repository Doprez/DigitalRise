﻿using DigitalRise.Animation;
using DigitalRise.Graphics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace Samples.Animation
{
  [Sample(SampleCategory.Animation,
    "This sample uses a custom animation class to create a circular movement.",
    "See also MyCircleAnimation.cs",
    5)]
  public class CustomAnimationSample : AnimationSample
  {
    private readonly AnimatableProperty<Vector2> _animatablePosition = new AnimatableProperty<Vector2>();


    public CustomAnimationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Start the custom circle animation.
      AnimationService.StartAnimation(new MyCircleAnimation(), _animatablePosition)
                      .UpdateAndApply();
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.White);

      // Draw sprite centered at the animated position.
      Vector2 position = (Vector2)_animatablePosition.Value - new Vector2(Logo.Width, Logo.Height) / 2.0f;

      SpriteBatch.Begin();
      SpriteBatch.Draw(Logo, position, Color.Red);
      SpriteBatch.End();
    }
  }
}

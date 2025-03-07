﻿using System.Collections.Generic;
using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DigitalRise
{
	internal static class Resources
	{
#if FNA
		private const string StockEffectsPrefix = "EffectsSource.FNA.bin";
#elif OPENGL
		private const string StockEffectsPrefix = "EffectsSource.MonoGameOGL.bin";
#else
		private const string StockEffectsPrefix = "EffectsSource.MonoGameDX11.bin";
#endif

		private static Texture2D _normalsFittingTexture;
		private static AssetManager _assetManagerEffects = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, StockEffectsPrefix);
		private static AssetManager _assetManagerResources = AssetManager.CreateResourceAssetManager(typeof(Resources).Assembly, "Resources");

		public static Effect GetDREffect(GraphicsDevice graphicsDevice, string path, Dictionary<string, string> defs = null)
		{
			path = path.Replace('\\', '/');
			if (path.StartsWith("DigitalRise/"))
			{
				path = path.Substring(12);
			}

			if (!path.EndsWith(".efb"))
			{
				path += ".efb";
			}

			return _assetManagerEffects.LoadEffect(graphicsDevice, path, defs);
		}

		public static Texture2D NormalsFittingTexture(GraphicsDevice graphicsDevice)
		{
			if (_normalsFittingTexture == null)
			{
				_normalsFittingTexture = _assetManagerResources.LoadTexture2D(graphicsDevice, "NormalsFittingTexture.png");
			}

			return _normalsFittingTexture;
		}
	}
}

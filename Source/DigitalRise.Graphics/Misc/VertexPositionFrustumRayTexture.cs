﻿// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Graphics
{
	/// <summary>
	/// Describes a custom vertex format structure that contains position and normal vector.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct VertexPositionFrustumRayTexture : IVertexType
	{
		//--------------------------------------------------------------
		#region Fields
		//--------------------------------------------------------------

		/// <summary>
		/// The vertex declaration.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
		public static readonly VertexDeclaration VertexDeclaration =
			new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
														new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
														new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0))
			{
				Name = "VertexPositionTextureFrustrumRay.VertexDeclaration"
			};


		/// <summary>
		/// The vertex position.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Vector3 Position;

		/// <summary>
		/// The vertex frustrum ray vector.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Vector3 FrustumRay;

		/// <summary>
		/// The texture coordinate.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		public Vector2 TextureCoordinate;

		#endregion


		//--------------------------------------------------------------
		#region Properties & Events
		//--------------------------------------------------------------

		/// <summary>
		/// Gets the size of the <see cref="VertexPositionFrustumRayTexture"/> structure in bytes.
		/// </summary>
		/// <value>The size of the vertex in bytes.</value>
		public static int SizeInBytes
		{
			get { return 12 + 12 + 9; }
		}
		#endregion


		//--------------------------------------------------------------
		#region Creation & Cleanup
		//--------------------------------------------------------------

		/// <summary>
		/// Initializes a new instance of the <see cref="VertexPositionFrustumRayTexture"/> struct.
		/// </summary>
		/// <param name="position">The position of the vertex.</param>
		/// <param name="frustrumRay">The normal of the vertex.</param>
		/// <param name="textureCoord">The texture coordinate of the vertex.</param>
		public VertexPositionFrustumRayTexture(Vector3 position, Vector3 frustrumRay, Vector2 textureCoord)
		{
			Position = position;
			FrustumRay = frustrumRay;
			TextureCoordinate = textureCoord;
		}
		#endregion


		//--------------------------------------------------------------
		#region Methods
		//--------------------------------------------------------------

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and 
		/// represent the same value; otherwise, <see langword="false"/>.
		/// </returns>
		public override bool Equals(object obj)
		{
			return obj is VertexPositionFrustumRayTexture && this == (VertexPositionFrustumRayTexture)obj;
		}


		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			// ReSharper disable NonReadonlyFieldInGetHashCode
			unchecked
			{
				int hashCode = Position.GetHashCode();
				hashCode = (hashCode * 397) ^ FrustumRay.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
				return hashCode;
			}
			// ReSharper restore NonReadonlyFieldInGetHashCode
		}


		/// <summary>
		/// Gets the vertex declaration.
		/// </summary>
		/// <value>The vertex declaration.</value>
		VertexDeclaration IVertexType.VertexDeclaration
		{
			get { return VertexDeclaration; }
		}


		/// <summary>
		/// Compares two objects to determine whether they are the same. 
		/// </summary>
		/// <param name="left">Object to the left of the equality operator.</param>
		/// <param name="right">Object to the right of the equality operator.</param>
		/// <returns>
		/// <see langword="true"/> if the objects are the same; <see langword="false"/> otherwise. 
		/// </returns>
		public static bool operator ==(VertexPositionFrustumRayTexture left, VertexPositionFrustumRayTexture right)
		{
			return (left.Position == right.Position)
						 && (left.FrustumRay == right.FrustumRay)
						 && (left.TextureCoordinate == right.TextureCoordinate);
		}


		/// <summary>
		/// Compares two objects to determine whether they are different. 
		/// </summary>
		/// <param name="left">Object to the left of the inequality operator.</param>
		/// <param name="right">Object to the right of the inequality operator.</param>
		/// <returns>
		/// <see langword="true"/> if the objects are different; <see langword="false"/> otherwise. 
		/// </returns>
		public static bool operator !=(VertexPositionFrustumRayTexture left, VertexPositionFrustumRayTexture right)
		{
			return (left.Position != right.Position)
						 || (left.FrustumRay != right.FrustumRay)
						 || (left.TextureCoordinate != right.TextureCoordinate);
		}


		/// <summary>
		/// Retrieves a string representation of this object.
		/// </summary>
		/// <returns>String representation of this object.</returns>
		public override string ToString()
		{
			return string.Format(
				CultureInfo.CurrentCulture,
				"{{Position:{0} FrustumRay:{1} TextureCoordinate:{2}}}",
				Position,
				FrustumRay,
				TextureCoordinate);
		}
		#endregion
	}
}

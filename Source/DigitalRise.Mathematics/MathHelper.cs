// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DigitalRise.Mathematics
{
	/// <summary>
	/// Provides useful mathematical algorithms and functions.
	/// </summary>
	public static class MathHelper
	{
		public static readonly Quaternion QuaternionZero = new Quaternion(0, 0, 0, 0);

		/// <summary>
		/// Clamps the specified value.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value which should be clamped.</param>
		/// <param name="min">The min limit.</param>
		/// <param name="max">The max limit.</param>
		/// <returns>
		/// <paramref name="value"/> clamped to the interval
		/// [<paramref name="min"/>, <paramref name="max"/>].
		/// </returns>
		/// <remarks>
		/// Values within the limits are not changed. Values exceeding the limits are cut off.
		/// </remarks>
		public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
		{
			if (min.CompareTo(max) > 0)
			{
				// min and max are swapped.
				var dummy = max;
				max = min;
				min = dummy;
			}

			if (value.CompareTo(min) < 0)
				value = min;
			else if (value.CompareTo(max) > 0)
				value = max;

			return value;
		}


		/// <overloads>
		/// <summary>
		/// Computes Sqrt(a*a + b*b) without underflow/overflow.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Computes Sqrt(a*a + b*b) without underflow/overflow (single-precision).
		/// </summary>
		/// <param name="cathetusA">Cathetus a.</param>
		/// <param name="cathetusB">Cathetus b.</param>
		/// <returns>The hypotenuse c, which is Sqrt(a*a + b*b).</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static float Hypotenuse(float cathetusA, float cathetusB)
		{
			float h = 0;
			if (Math.Abs(cathetusA) > Math.Abs(cathetusB))
			{
				h = cathetusB / cathetusA;
				h = (float)(Math.Abs(cathetusA) * Math.Sqrt(1 + h * h));
			}
			else if (cathetusB != 0)
			{
				h = cathetusA / cathetusB;
				h = (float)(Math.Abs(cathetusB) * Math.Sqrt(1 + h * h));
			}

			return h;
		}


		/// <summary>
		/// Computes Sqrt(a*a + b*b) without underflow/overflow (double-precision).
		/// </summary>
		/// <param name="cathetusA">Cathetus a.</param>
		/// <param name="cathetusB">Cathetus b.</param>
		/// <returns>The hypotenuse c, which is Sqrt(a*a + b*b).</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static double Hypotenuse(double cathetusA, double cathetusB)
		{
			double h = 0;
			if (Math.Abs(cathetusA) > Math.Abs(cathetusB))
			{
				h = cathetusB / cathetusA;
				h = Math.Abs(cathetusA) * Math.Sqrt(1 + h * h);
			}
			else if (cathetusB != 0)
			{
				h = cathetusA / cathetusB;
				h = Math.Abs(cathetusB) * Math.Sqrt(1 + h * h);
			}

			return h;
		}


		/// <summary>
		/// Swaps the content of two variables.
		/// </summary>
		/// <typeparam name="T">The type of the objects.</typeparam>
		/// <param name="obj1">First variable.</param>
		/// <param name="obj2">Second variable.</param>
		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static void Swap<T>(ref T obj1, ref T obj2)
		{
			T temp = obj1;
			obj1 = obj2;
			obj2 = temp;
		}


		/// <overloads>
		/// <summary>
		/// Converts an angle value from degrees to radians.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Converts an angle value from degrees to radians (single-precision).
		/// </summary>
		/// <param name="degree">The angle in degrees.</param>
		/// <returns>The angle in radians.</returns>
		public static float ToRadians(float degree)
		{
			return degree * ConstantsF.Pi / 180;
		}


		/// <overloads>
		/// <summary>
		/// Converts an angle value from radians to degrees.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Converts an angle value from radians to degrees (single-precision).
		/// </summary>
		/// <param name="radians">The angle in radians.</param>
		/// <returns>The angle in degrees.</returns>
		public static float ToDegrees(float radians)
		{
			return radians * 180 * ConstantsF.OneOverPi;
		}


		/// <summary>
		/// Returns the largest non-negative integer x such that 2<sup>x</sup> ≤ <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// The largest non-negative integer x such that 2<sup>x</sup> ≤ <paramref name="value"/>.
		/// Exception: If <paramref name="value"/> is 0 then 0 is returned.
		/// </returns>
		[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
		public static uint Log2LessOrEqual(uint value)
		{
			// See Game Programming Gems 3, "Fast Base-2 Functions for Logarithms and Random Number Generation.

			uint testValue; // The value against which we test in the if condition.
			uint x;         // The value we are looking for.

			if (value >= 0x10000)
			{
				x = 16;
				testValue = 0x1000000;
			}
			else
			{
				x = 0;
				testValue = 0x100;
			}

			if (value >= testValue)
			{
				x += 8;
				testValue <<= 4;
			}
			else
			{
				testValue >>= 4;
			}

			if (value >= testValue)
			{
				x += 4;
				testValue <<= 2;
			}
			else
			{
				testValue >>= 2;
			}

			if (value >= testValue)
			{
				x += 2;
				testValue <<= 1;
			}
			else
			{
				testValue >>= 1;
			}

			if (value >= testValue)
			{
				x += 1;
			}

			return x;
		}


		/// <summary>
		/// Returns the smallest non-negative integer x such that 2<sup>x</sup> ≥ <paramref name="value"/>.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// The smallest non-negative integer x such that 2<sup>x</sup> ≥ <paramref name="value"/>.
		/// Exception: If <paramref name="value"/> is 0, 0 is returned.
		/// </returns>
		public static uint Log2GreaterOrEqual(uint value)
		{
			// See Game Programming Gems 3, "Fast Base-2 Functions for Logarithms and Random Number Generation.
			if (value > 0x80000000)
				return 32;

			uint testValue; // The value against which we test in the if condition.
			uint x;         // The value we are looking for.

			if (value > 0x8000)
			{
				x = 16;
				testValue = 0x800000;
			}
			else
			{
				x = 0;
				testValue = 0x80;
			}

			if (value > testValue)
			{
				x += 8;
				testValue <<= 4;
			}
			else
			{
				testValue >>= 4;
			}

			if (value > testValue)
			{
				x += 4;
				testValue <<= 2;
			}
			else
			{
				testValue >>= 2;
			}

			if (value > testValue)
			{
				x += 2;
				testValue <<= 1;
			}
			else
			{
				testValue >>= 1;
			}

			if (value > testValue)
			{
				x += 1;
			}

			return x;
		}


		/// <summary>
		/// Creates the smallest bitmask that is greater than or equal to the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// A bitmask where the left bits are 0 and the right bits are 1. The value of the bitmask
		/// is ≥ <paramref name="value"/>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// This result can also be interpreted as finding the smallest x such that 2<sup>x</sup> &gt; 
		/// <paramref name="value"/> and returning 2<sup>x</sup> - 1.
		/// </para>
		/// <para>
		/// Another useful application: Bitmask(x) + 1 returns the next power of 2 that is greater than 
		/// x.
		/// </para>
		/// </remarks>
		public static uint Bitmask(uint value)
		{
			// Example:                 value = 10000000 00000000 00000000 00000000
			value |= (value >> 1);   // value = 11000000 00000000 00000000 00000000
			value |= (value >> 2);   // value = 11110000 00000000 00000000 00000000
			value |= (value >> 4);   // value = 11111111 00000000 00000000 00000000
			value |= (value >> 8);   // value = 11111111 11111111 00000000 00000000
			value |= (value >> 16);  // value = 11111111 11111111 11111111 11111111
			return value;
		}


		/// <summary>
		/// Determines whether the specified value is a power of two.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="value"/> is a power of two; otherwise, 
		/// <see langword="false"/>.
		/// </returns>
		[SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "value-1")]
		public static bool IsPowerOf2(int value)
		{
			// See http://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
			return (value != 0) && (value & (value - 1)) == 0;
		}


		/// <summary>
		/// Returns the smallest power of two that is greater than the given value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>
		/// The smallest power of two (2<sup>x</sup>) that is greater than <paramref name="value"/>.
		/// </returns>
		/// <remarks>
		/// For example, <c>NextPowerOf2(7)</c> is <c>8</c> and <c>NextPowerOf2(8)</c> is <c>16</c>.
		/// </remarks>
		public static uint NextPowerOf2(uint value)
		{
			return Bitmask(value) + 1;
		}


		/// <overloads>
		/// <summary>
		/// Computes the Gaussian function y = k * e^( -(x-μ)<sup>2</sup>/(2σ<sup>2</sup>).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Computes the Gaussian function y = k * e^( -(x-μ)<sup>2</sup>/(2σ<sup>2</sup>) 
		/// (single precision).
		/// </summary>
		/// <param name="x">The argument x.</param>
		/// <param name="coefficient">The coefficient k.</param>
		/// <param name="expectedValue">The expected value μ.</param>
		/// <param name="standardDeviation">The standard deviation σ.</param>
		/// <returns>The height of the Gaussian bell curve at x.</returns>
		/// <remarks>
		/// This method computes the Gaussian bell curve.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static float Gaussian(float x, float coefficient, float expectedValue, float standardDeviation)
		{
			float xMinusExpected = x - expectedValue;
			return coefficient * (float)Math.Exp(-xMinusExpected * xMinusExpected
																					 / (2 * standardDeviation * standardDeviation));
		}


		/// <summary>
		/// <summary>
		/// Computes the Gaussian function y = k * e^( -(x-μ)<sup>2</sup>/(2σ<sup>2</sup>) 
		/// (double-precision).
		/// </summary>
		/// </summary>
		/// <param name="x">The argument x.</param>
		/// <param name="coefficient">The coefficient k.</param>
		/// <param name="expectedValue">The expected value μ.</param>
		/// <param name="standardDeviation">The standard deviation σ.</param>
		/// <returns>The height of the Gaussian bell curve at x.</returns>
		/// <remarks>
		/// This method computes the Gaussian bell curve.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static double Gaussian(double x, double coefficient, double expectedValue, double standardDeviation)
		{
			double xMinusExpected = x - expectedValue;
			return coefficient * Math.Exp(-xMinusExpected * xMinusExpected
																		/ (2 * standardDeviation * standardDeviation));
		}


		/// <summary>
		/// Computes the binomial coefficient of (n, k), also read as "n choose k".
		/// </summary>
		/// <param name="n">n, must be a value equal to or greater than 0.</param>
		/// <param name="k">k, a value in the range [0, <paramref name="n"/>].</param>
		/// <returns>
		/// The binomial coefficient.
		/// </returns>
		/// <remarks>
		/// This method returns a binomial coefficient. The result is the k'th element in the n'th row
		/// of Pascal's triangle (using zero-based indices for k and n). This method returns 0 for
		/// negative <paramref name="n"/>.
		/// </remarks>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static long BinomialCoefficient(int n, int k)
		{
			// See http://blog.plover.com/math/choose.html.

			if (k < 0 || k > n)
				return 0;

			long r = 1;
			long d;
			for (d = 1; d <= k; d++)
			{
				r *= n--;
				r /= d;
			}
			return r;
		}


		/// <overloads>
		/// <summary>
		/// Calculates the fractional part of a specified floating-point number.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Calculates the fractional part of a specified single-precision floating-point number.
		/// </summary>
		/// <param name="f">The number.</param>
		/// <returns>The fractional part of <paramref name="f"/>.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static float Frac(float f)
		{
			return f - (float)Math.Floor(f);
		}


		/// <summary>
		/// Calculates the fractional part of a specified double-precision floating-point number.
		/// </summary>
		/// <param name="d">The number.</param>
		/// <returns>The fractional part of <paramref name="d"/>.</returns>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
		public static double Frac(double d)
		{
			return d - Math.Floor(d);
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two vectors are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether two vectors are equal (regarding the tolerance 
		/// <see cref="Numeric.EpsilonF"/>).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <see cref="Numeric.EpsilonF"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector2 vector1, Vector2 vector2)
		{
			return Numeric.AreEqual(vector1.X, vector2.X)
					&& Numeric.AreEqual(vector1.Y, vector2.Y);
		}

		/// <summary>
		/// Determines whether two vectors are equal (regarding a specific tolerance).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <param name="epsilon">The tolerance value.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <paramref name="epsilon"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <paramref name="epsilon"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector2 vector1, Vector2 vector2, float epsilon)
		{
			return Numeric.AreEqual(vector1.X, vector2.X, epsilon)
					&& Numeric.AreEqual(vector1.Y, vector2.Y, epsilon);
		}


		public static float GetComponentByIndex(this Vector2 a, int index)
		{
			switch (index)
			{
				case 0: return a.X;
				case 1: return a.Y;
			}

			throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 and 1.");
		}

		public static void SetComponentByIndex(this ref Vector2 a, int index, float value)
		{
			switch (index)
			{
				case 0: a.X = value; break;
				case 1: a.Y = value; break;
				default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 and 1.");
			}
		}


		public static bool IsLessThen(this Vector2 a, Vector2 b) => a.X < b.X && a.Y < b.Y;
		public static bool IsLessOrEqual(this Vector2 a, Vector2 b) => a.X <= b.X && a.Y <= b.Y;
		public static bool IsGreaterThen(this Vector2 a, Vector2 b) => a.X > b.X && a.Y > b.Y;
		public static bool IsGreaterOrEqual(this Vector2 a, Vector2 b) => a.X >= b.X && a.Y >= b.Y;


		/// <summary>
		/// Gets a value indicating whether a component of the vector is <see cref="float.NaN"/>.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if a component of the vector is <see cref="float.NaN"/>; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		public static bool IsNaN(this Vector2 a) => Numeric.IsNaN(a.X) || Numeric.IsNaN(a.Y);


		/// <summary>
		/// Tries to normalize the vector.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the vector was normalized; otherwise, <see langword="false"/> if 
		/// the vector could not be normalized. (The length is numerically zero.)
		/// </returns>
		public static bool TryNormalize(this ref Vector2 a)
		{
			float lengthSquared = a.LengthSquared();
			if (Numeric.IsZero(lengthSquared, Numeric.EpsilonFSquared))
				return false;

			float length = (float)Math.Sqrt(lengthSquared);

			float scale = 1.0f / length;
			a.X *= scale;
			a.Y *= scale;

			return true;
		}

		/// <summary>
		/// Returns the normalized vector.
		/// </summary>
		/// <value>The normalized vector.</value>
		/// <remarks>
		/// The property does not change this instance. To normalize this instance you need to call 
		/// <see cref="Normalize"/>.
		/// </remarks>
		/// <exception cref="DivideByZeroException">
		/// The length of the vector is zero. The quaternion cannot be normalized.
		/// </exception>
		public static Vector2 Normalized(this Vector2 a)
		{
			Vector2 v = a;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Returns a vector with the absolute values of the elements of the given vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>A vector with the absolute values of the elements of the given vector.</returns>
		public static Vector2 Absolute(Vector2 vector)
		{
			return new Vector2(Math.Abs(vector.X), Math.Abs(vector.Y));
		}

		/// <summary>
		/// Returns a vector with the vector components clamped to the range [min, max].
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="min">The min limit.</param>
		/// <param name="max">The max limit.</param>
		/// <returns>A vector with clamped components.</returns>
		/// <remarks>
		/// This operation is carried out per component. Component values less than 
		/// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
		/// <paramref name="max"/> are set to <paramref name="max"/>.
		/// </remarks>
		public static Vector2 Clamp(Vector2 vector, float min, float max)
		{
			return new Vector2(MathHelper.Clamp(vector.X, min, max),
													MathHelper.Clamp(vector.Y, min, max));
		}

		/// <summary>
		/// Returns a value indicating whether this vector has zero size (the length is numerically
		/// equal to 0).
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this vector is numerically zero; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// The length of this vector is compared to 0 using the default tolerance value (see 
		/// <see cref="Numeric.EpsilonD"/>).
		/// </remarks>
		public static bool IsNumericallyZero(this Vector3 a) => Numeric.IsZero(a.LengthSquared(), Numeric.EpsilonFSquared);

		/// <summary>
		/// Projects a vector onto an axis given by the target vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <param name="target">The target vector.</param>
		/// <returns>
		/// The projection of <paramref name="vector"/> onto <paramref name="target"/>.
		/// </returns>
		public static Vector3 ProjectTo(Vector3 vector, Vector3 target)
		{
			return Vector3.Dot(vector, target) / target.LengthSquared() * target;
		}

		/// <summary>
		/// Tries to normalize the vector.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the vector was normalized; otherwise, <see langword="false"/> if 
		/// the vector could not be normalized. (The length is numerically zero.)
		/// </returns>
		public static bool TryNormalize(this ref Vector3 a)
		{
			float lengthSquared = a.LengthSquared();
			if (Numeric.IsZero(lengthSquared, Numeric.EpsilonDSquared))
				return false;

			float length = MathF.Sqrt(lengthSquared);

			float scale = 1.0f / length;
			a.X *= scale;
			a.Y *= scale;
			a.Z *= scale;

			return true;
		}

		/// <summary>
		/// Returns the normalized vector.
		/// </summary>
		/// <value>The normalized vector.</value>
		/// <remarks>
		/// The property does not change this instance. To normalize this instance you need to call 
		/// <see cref="Normalize"/>.
		/// </remarks>
		/// <exception cref="DivideByZeroException">
		/// The length of the vector is zero. The quaternion cannot be normalized.
		/// </exception>
		public static Vector3 Normalized(this Vector3 a)
		{
			Vector3 v = a;
			v.Normalize();
			return v;
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two vectors are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether two vectors are equal (regarding the tolerance 
		/// <see cref="Numeric.EpsilonD"/>).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <see cref="Numeric.EpsilonD"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <see cref="Numeric.EpsilonD"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector3 vector1, Vector3 vector2)
		{
			return Numeric.AreEqual(vector1.X, vector2.X)
					&& Numeric.AreEqual(vector1.Y, vector2.Y)
					&& Numeric.AreEqual(vector1.Z, vector2.Z);
		}

		/// <summary>
		/// Determines whether two vectors are equal (regarding a specific tolerance).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <param name="epsilon">The tolerance value.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <paramref name="epsilon"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <paramref name="epsilon"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector3 vector1, Vector3 vector2, float epsilon)
		{
			return Numeric.AreEqual(vector1.X, vector2.X, epsilon)
					&& Numeric.AreEqual(vector1.Y, vector2.Y, epsilon)
					&& Numeric.AreEqual(vector1.Z, vector2.Z, epsilon);
		}

		/// <summary>
		/// Returns an arbitrary normalized <see cref="Vector3F"/> that is orthogonal to this vector.
		/// </summary>
		/// <value>An arbitrary normalized orthogonal <see cref="Vector3F"/>.</value>
		public static Vector3 Orthonormal1(this Vector3 a)
		{
			// Note: Other options to create normal vectors are discussed here:
			// http://blog.selfshadow.com/2011/10/17/perp-vectors/,
			// http://box2d.org/2014/02/computing-a-basis/
			// and here
			// "Building an Orthonormal Basis from a 3D Unit Vector Without Normalization"
			// http://orbit.dtu.dk/fedora/objects/orbit:113874/datastreams/file_75b66578-222e-4c7d-abdf-f7e255100209/content
			// This method is implemented in DigitalRune.Graphics/Misc.fxh/GetOrthonormals().

			Vector3 v;
			if (Numeric.IsZero(a.Z) == false)
			{
				// Orthonormal = (1, 0, 0) x (X, Y, Z)
				v.X = 0f;
				v.Y = -a.Z;
				v.Z = a.Y;
			}
			else
			{
				// Orthonormal = (0, 0, 1) x (X, Y, Z)
				v.X = -a.Y;
				v.Y = a.X;
				v.Z = 0f;
			}
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Gets a normalized orthogonal <see cref="Vector3F"/> that is orthogonal to this 
		/// <see cref="Vector3F"/> and to <see cref="Orthonormal1"/>.
		/// </summary>
		/// <value>
		/// A normalized orthogonal <see cref="Vector3F"/> which is orthogonal to this 
		/// <see cref="Vector3F"/> and to <see cref="Orthonormal1"/>.
		/// </value>
		public static Vector3 Orthonormal2(this Vector3 a)
		{
			Vector3 v = Vector3.Cross(a, Orthonormal1(a));
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Returns a vector with the absolute values of the elements of the given vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>A vector with the absolute values of the elements of the given vector.</returns>
		public static Vector3 Absolute(Vector3 vector)
		{
			return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
		}

		/// <summary>
		/// Returns a value indicating whether this vector is normalized (the length is numerically
		/// equal to 1).
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this <see cref="Vector3F"/> is normalized; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// <see cref="IsNumericallyNormalized"/> compares the length of this vector against 1.0 using
		/// the default tolerance value (see <see cref="Numeric.EpsilonF"/>).
		/// </remarks>
		public static bool IsNumericallyNormalized(this Vector3 a)
		{
			return Numeric.AreEqual(a.LengthSquared(), 1.0f);
		}


		public static float GetComponentByIndex(this Vector3 a, int index)
		{
			switch (index)
			{
				case 0: return a.X;
				case 1: return a.Y;
				case 2: return a.Z;
			}

			throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0, 1, or 2.");
		}

		public static void SetComponentByIndex(this ref Vector3 a, int index, float value)
		{
			switch (index)
			{
				case 0: a.X = value; break;
				case 1: a.Y = value; break;
				case 2: a.Z = value; break;
				default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0, 1, or 2.");
			}
		}

		public static bool IsLessThen(this Vector3 a, Vector3 b) => a.X < b.X && a.Y < b.Y && a.Z < b.Z;
		public static bool IsLessOrEqual(this Vector3 a, Vector3 b) => a.X <= b.X && a.Y <= b.Y && a.Z <= b.Z;
		public static bool IsGreaterThen(this Vector3 a, Vector3 b) => a.X > b.X && a.Y > b.Y && a.Z > b.Z;
		public static bool IsGreaterOrEqual(this Vector3 a, Vector3 b) => a.X >= b.X && a.Y >= b.Y && a.Z >= b.Z;

		/// <summary>
		/// Gets the index (zero-based) of the largest component.
		/// </summary>
		/// <value>The index (zero-based) of the largest component.</value>
		/// <remarks>
		/// <para>
		/// This method returns the index of the component (X, Y or Z) which has the largest value. The 
		/// index is zero-based, i.e. the index of X is 0. 
		/// </para>
		/// <para>
		/// If there are several components with equally large values, the smallest index of these is 
		/// returned.
		/// </para>
		/// </remarks>
		public static int IndexOfLargestComponent(this Vector3 a)
		{
			if (a.X >= a.Y && a.X >= a.Z)
				return 0;

			if (a.Y >= a.Z)
				return 1;

			return 2;
		}

		/// <summary>
		/// Gets the value of the largest component.
		/// </summary>
		/// <value>The value of the largest component.</value>
		public static float LargestComponent(this Vector3 a)
		{
			if (a.X >= a.Y && a.X >= a.Z)
				return a.X;

			if (a.Y >= a.Z)
				return a.Y;

			return a.Z;
		}

		/// <summary>
		/// Gets the value of the smallest component.
		/// </summary>
		/// <value>The value of the smallest component.</value>
		public static float SmallestComponent(this Vector3 a)
		{
			if (a.X <= a.Y && a.X <= a.Z)
				return a.X;

			if (a.Y <= a.Z)
				return a.Y;

			return a.Z;
		}


		/// <summary>
		/// Gets the index (zero-based) of the largest component.
		/// </summary>
		/// <value>The index (zero-based) of the largest component.</value>
		/// <remarks>
		/// <para>
		/// This method returns the index of the component (X, Y or Z) which has the smallest value. The 
		/// index is zero-based, i.e. the index of X is 0. 
		/// </para>
		/// <para>
		/// If there are several components with equally small values, the smallest index of these is 
		/// returned.
		/// </para>
		/// </remarks>
		public static int IndexOfSmallestComponent(this Vector3 a)
		{
			if (a.X <= a.Y && a.X <= a.Z)
				return 0;

			if (a.Y <= a.Z)
				return 1;

			return 2;
		}

		/// <summary>
		/// Gets a value indicating whether a component of the vector is <see cref="float.NaN"/>.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if a component of the vector is <see cref="float.NaN"/>; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		public static bool IsNaN(this Vector3 a)
		{
			return Numeric.IsNaN(a.X) || Numeric.IsNaN(a.Y) || Numeric.IsNaN(a.Z);
		}

		/// <summary>
		/// Returns the cross product matrix (skew matrix) of this vector.
		/// </summary>
		/// <returns>The cross product matrix of this vector.</returns>
		/// <remarks>
		/// <c>Vector3F.Cross(v, w)</c> is the same as <c>v.ToCrossProductMatrix() * w</c>.
		/// </remarks>
		public static Matrix33F ToCrossProductMatrix(this Vector3 a)
		{
			return new Matrix33F(0, -a.Z, a.Y,
													 a.Z, 0, -a.X,
													 -a.Y, a.X, 0);
		}

		public static void SetLength(this ref Vector3 a, float value)
		{
			float length = a.Length();
			if (Numeric.IsZero(length))
				throw new MathematicsException("Cannot change length of a vector with length 0.");

			float scale = value / length;
			a.X *= scale;
			a.Y *= scale;
			a.Z *= scale;
		}

		/// <overloads>
		/// <summary>
		/// Clamps the vector components to the range [min, max].
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Clamps the vector components to the range [min, max].
		/// </summary>
		/// <param name="min">The min limit.</param>
		/// <param name="max">The max limit.</param>
		/// <remarks>
		/// This operation is carried out per component. Component values less than 
		/// <paramref name="min"/> are set to <paramref name="min"/>. Component values greater than 
		/// <paramref name="max"/> are set to <paramref name="max"/>.
		/// </remarks>
		public static Vector3 Clamp(Vector3 a, float min, float max)
		{
			a.X = MathHelper.Clamp(a.X, min, max);
			a.Y = MathHelper.Clamp(a.Y, min, max);
			a.Z = MathHelper.Clamp(a.Z, min, max);

			return a;
		}

		/// <summary>
		/// Calculates the angle between two vectors.
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>The angle between the given vectors, such that 0 ≤ angle ≤ π.</returns>
		/// <exception cref="ArgumentException">
		/// <paramref name="vector1"/> or <paramref name="vector2"/> has a length of 0.
		/// </exception>
		public static float GetAngle(Vector3 vector1, Vector3 vector2)
		{
			if (!vector1.TryNormalize() || !vector2.TryNormalize())
				throw new ArgumentException("vector1 and vector2 must not have 0 length.");

			float α = Vector3.Dot(vector1, vector2);

			// Inaccuracy in the floating-point operations can cause
			// the result be outside of the valid range.
			// Ensure that the dot product α lies in the interval [-1, 1].
			// Math.Acos() returns Double.NaN if the argument lies outside
			// of this interval.
			α = MathHelper.Clamp(α, -1.0f, 1.0f);

			return (float)Math.Acos(α);
		}

		/// <summary>
		/// Converts this vector to an array of 3 <see langword="float"/> values.
		/// </summary>
		/// <returns>
		/// The array with 3 <see langword="float"/> values. The order of the elements is: x, y, z
		/// </returns>
		public static float[] ToArray(this Vector3 a)
		{
			return new[] { a.X, a.Y, a.Z };
		}

		public static Vector3 XYZ(this Vector4 a) => new Vector3(a.X, a.Y, a.Z);

		public static float GetComponentByIndex(this Vector4 a, int index)
		{
			switch (index)
			{
				case 0: return a.X;
				case 1: return a.Y;
				case 2: return a.Z;
				case 3: return a.W;
			}

			throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
		}

		public static void SetComponentByIndex(this ref Vector4 a, int index, float value)
		{
			switch (index)
			{
				case 0: a.X = value; break;
				case 1: a.Y = value; break;
				case 2: a.Z = value; break;
				case 3: a.W = value; break;
				default: throw new ArgumentOutOfRangeException("index", "The index is out of range. Allowed values are 0 to 3.");
			}
		}

		/// <summary>
		/// Performs the homogeneous divide or perspective divide: X, Y and Z are divided by W.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The vector (X/W, Y/W, Z/W).</returns>
		/// <exception cref="DivideByZeroException">
		/// Component W is 0.
		/// </exception>
		public static Vector3 HomogeneousDivide(this Vector4 vector)
		{
			float w = vector.W;

			if (w == 1.0f)
				return new Vector3(vector.X, vector.Y, vector.Z);

			float oneOverW = 1 / w;
			return new Vector3(vector.X * oneOverW, vector.Y * oneOverW, vector.Z * oneOverW);
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two vectors are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Determines whether two vectors are equal (regarding the tolerance 
		/// <see cref="Numeric.EpsilonF"/>).
		/// </summary>
		/// <param name="vector1">The first vector.</param>
		/// <param name="vector2">The second vector.</param>
		/// <returns>
		/// <see langword="true"/> if the vectors are equal (within the tolerance 
		/// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// The two vectors are compared component-wise. If the differences of the components are less
		/// than <see cref="Numeric.EpsilonF"/> the vectors are considered as being equal.
		/// </remarks>
		public static bool AreNumericallyEqual(Vector4 vector1, Vector4 vector2)
		{
			return Numeric.AreEqual(vector1.X, vector2.X)
					&& Numeric.AreEqual(vector1.Y, vector2.Y)
					&& Numeric.AreEqual(vector1.Z, vector2.Z)
					&& Numeric.AreEqual(vector1.W, vector2.W);
		}

		/// <summary>
		/// Returns the normalized vector.
		/// </summary>
		/// <value>The normalized vector.</value>
		/// <remarks>
		/// The property does not change this instance. To normalize this instance you need to call 
		/// <see cref="Normalize"/>.
		/// </remarks>
		/// <exception cref="DivideByZeroException">
		/// The length of the vector is zero. The quaternion cannot be normalized.
		/// </exception>
		public static Vector4 Normalized(this Vector4 a)
		{
			Vector4 v = a;
			v.Normalize();
			return v;
		}

		/// <summary>
		/// Creates a unit quaternion that specifies the same rotation as the given rotation matrix.
		/// </summary>
		/// <param name="rotationMatrix">A orientation matrix that specifies a rotation.</param>
		/// <returns>
		/// The creates unit quaternion that describes the same rotation as the rotation matrix.
		/// </returns>
		/// <remarks>
		/// The given matrix is converted into a unit quaternion that specifies the same rotation.
		/// </remarks>
		public static Quaternion CreateRotation(Matrix33F rotationMatrix)
		{
			// Credits: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
			// Note: A less general branchless implementation is discussed here: http://www.thetenthplanet.de/archives/1994

			float x, y, z, w;

			// Calculate diagonal sum (= trace) of the matrix.
			float trace = rotationMatrix.M00 + rotationMatrix.M11 + rotationMatrix.M22;
			if (trace > 0)
			{
				float s = 0.5f / (float)Math.Sqrt(trace + 1.0f);
				w = 0.25f / s;
				x = (rotationMatrix.M21 - rotationMatrix.M12) * s;
				y = (rotationMatrix.M02 - rotationMatrix.M20) * s;
				z = (rotationMatrix.M10 - rotationMatrix.M01) * s;
			}
			else
			{
				if (rotationMatrix.M00 > rotationMatrix.M11 && rotationMatrix.M00 > rotationMatrix.M22)
				{
					float s = 2.0f * (float)Math.Sqrt(1.0f + rotationMatrix.M00 - rotationMatrix.M11 - rotationMatrix.M22);
					w = (rotationMatrix.M21 - rotationMatrix.M12) / s;
					x = 0.25f * s;
					y = (rotationMatrix.M01 + rotationMatrix.M10) / s;
					z = (rotationMatrix.M02 + rotationMatrix.M20) / s;
				}
				else if (rotationMatrix.M11 > rotationMatrix.M22)
				{
					float s = 2.0f * (float)Math.Sqrt(1.0f + rotationMatrix.M11 - rotationMatrix.M00 - rotationMatrix.M22);
					w = (rotationMatrix.M02 - rotationMatrix.M20) / s;
					x = (rotationMatrix.M01 + rotationMatrix.M10) / s;
					y = 0.25f * s;
					z = (rotationMatrix.M12 + rotationMatrix.M21) / s;
				}
				else
				{
					float s = 2.0f * (float)Math.Sqrt(1.0f + rotationMatrix.M22 - rotationMatrix.M00 - rotationMatrix.M11);
					w = (rotationMatrix.M10 - rotationMatrix.M01) / s;
					x = (rotationMatrix.M02 + rotationMatrix.M20) / s;
					y = (rotationMatrix.M12 + rotationMatrix.M21) / s;
					z = 0.25f * s;
				}
			}

			return new Quaternion(x, y, z, w);
		}

		/// <summary>
		/// Returns the inverse of this quaternion.
		/// </summary>
		/// <value>The inverse of this quaternion.</value>
		/// <remarks>
		/// <para>
		/// The (multiplicative) inverse of a quaternion is calculated by using the following formula:
		/// </para>
		/// <para>
		/// <i>q<sup>-1</sup> = q<sup>*</sup> / (q q<sup>*</sup>) = q<sup>*</sup> / </i>N(<i>q</i>)
		/// </para>
		/// <para>
		/// The property does not change this quaternion. To invert this instance you need to call 
		/// <see cref="Invert"/>.
		/// </para>
		/// <para>
		/// The inverse of a unit quaternion is the same as its conjugate. You might consider using the
		/// property <see cref="Conjugated"/> because it is faster than <see cref="Inverse"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="MathematicsException">
		/// The length of the quaternion is zero. The quaternion cannot be inverted.
		/// </exception>
		public static Quaternion Inverse(this Quaternion q)
		{
			return Quaternion.Inverse(q);
		}

		/// <summary>
		/// Calculates the exponential.
		/// </summary>
		/// <param name="quaternion">The quaternion.</param>
		/// <returns>The exponential e<sup><i>q</i></sup>.</returns>
		/// <remarks>
		/// <para>
		/// <strong>Important:</strong> This method requires that the quaternion is a pure quaternion. A
		/// pure quaternion is defined by <i>q</i> = (0, <i><b>u</b>θ</i>) where <i><b>u</b></i> is a
		/// unit vector.
		/// </para>
		/// <para>
		/// The exponential of a quaternion <i>q</i> is defines as:
		/// </para>
		/// <para>
		/// e<sup><i>q</i></sup> = (cos<i>θ</i> + <i><b>u</b></i>sin<i>θ</i>)
		/// </para>
		/// <para>
		/// The result is returned as a quaternion with the form: 
		/// (cos(<i>θ</i>), <i><b>u</b></i>sin(<i>θ</i>))
		/// </para>
		/// </remarks>
		public static Quaternion Exp(this Quaternion quaternion)
		{
			float θ = (float)Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
			float cosθ = (float)Math.Cos(θ);

			if (θ > Numeric.EpsilonF)
			{
				float coefficient = (float)Math.Sin(θ) / θ;
				quaternion.W = cosθ;
				quaternion.X *= coefficient;
				quaternion.Y *= coefficient;
				quaternion.Z *= coefficient;
			}
			else
			{
				// In this case θ was 0.
				// Therefore: cos(θ) = 1, sin(θ) = 0
				quaternion.W = cosθ;

				// We do not have to set (x, y, z) because we already know that length
				// is 0.
			}
			return quaternion;
		}

		/// <summary>
		/// Calculates the natural logarithm.
		/// </summary>
		/// <param name="quaternion">The quaternion.</param>
		/// <returns>The natural logarithm ln(<i>q</i>).</returns>
		/// <remarks>
		/// <para>
		/// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
		/// </para>
		/// <para>
		/// The natural logarithm of a quaternion <i>q</i> is defines as:
		/// </para>
		/// <para>
		/// ln(<i>q</i>) = ln(cos(<i>θ</i>) + <i><b>u</b></i>sin(<i>θ</i>)) 
		///              = ln(e<sup><i><b>u</b>θ</i></sup>) = <i><b>u</b>θ</i>
		/// </para>
		/// <para>
		/// The result is returned as a quaternion with the form: (0, <i><b>u</b>θ</i>)
		/// </para>
		/// </remarks>
		/// <exception cref="MathematicsException">
		/// The given quaternion is not a unit quaternion.
		/// </exception>
		public static Quaternion Ln(this Quaternion quaternion)
		{
			if (Numeric.IsLessOrEqual(Math.Abs(quaternion.W), 1.0f))
			{
				float sinθ = (float)Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
				float θ = (float)Math.Atan(sinθ / quaternion.W);

				// Slower version:
				//float θ = System.Math.Acos(quaternion.W);
				//float sinθ = System.Math.Sin(θ);

				if (!Numeric.IsZero(sinθ))
				{
					float coefficient = θ / sinθ;
					quaternion.W = 0.0f;
					quaternion.X *= coefficient;
					quaternion.Y *= coefficient;
					quaternion.Z *= coefficient;
				}
				else
				{
					// In this case θ was 0.
					// cos(θ) = 1, sin(θ) = 0
					// We assume that the given quaternion is a unit quaternion.
					// If w = 1, then all other components should be 0.
					Debug.Assert(Numeric.IsZero(quaternion.X), "Quaternion is not a unit quaternion.");
					Debug.Assert(Numeric.IsZero(quaternion.Y), "Quaternion is not a unit quaternion.");
					Debug.Assert(Numeric.IsZero(quaternion.Z), "Quaternion is not a unit quaternion.");

					// Return (0, (0, 0, 0))
					quaternion.W = 0.0f;

					// We do not have to touch (x, y, z).
				}
				return quaternion;
			}
			else
			{
				throw new MathematicsException("The quaternion is not a unit quaternion. Ln only works for unit quaternions.");
			}
		}

		/// <summary>
		/// Returns the 3 x 3 rotation matrix of this quaternion.
		/// </summary>
		/// <returns>The rotation matrix.</returns>
		/// <remarks>
		/// The method assumes that this quaternion is a unit quaternion (i.e. that it is normalized).
		/// The unit quaternion specifies a rotation that can be converted into a corresponding 3 x 3
		/// rotation matrix.
		/// </remarks>
		public static Matrix33F ToRotationMatrix33(this Quaternion q)
		{
			Matrix33F m;

			float twoX = 2 * q.X;
			float twoY = 2 * q.Y;
			float twoZ = 2 * q.Z;
			float twoXX = twoX * q.X;
			float twoYY = twoY * q.Y;
			float twoZZ = twoZ * q.Z;
			float twoXY = twoX * q.Y;
			float twoXZ = twoX * q.Z;
			float twoYZ = twoY * q.Z;
			float twoXW = twoX * q.W;
			float twoYW = twoY * q.W;
			float twoZW = twoZ * q.W;

			// according to Watt, p.489
			m.M00 = 1 - (twoYY + twoZZ);
			m.M01 = twoXY - twoZW;
			m.M02 = twoYW + twoXZ;

			m.M10 = twoXY + twoZW;
			m.M11 = 1 - (twoXX + twoZZ);
			m.M12 = twoYZ - twoXW;

			m.M20 = twoXZ - twoYW;
			m.M21 = twoXW + twoYZ;
			m.M22 = 1 - (twoXX + twoYY);

			return m;
		}

		/// <summary>
		/// Returns the 4 x 4 rotation matrix of this quaternion.
		/// </summary>
		/// <returns>The rotation matrix.</returns>
		/// <remarks>
		/// <para>
		/// The method assumes that this quaternion is a unit quaternion (i.e. that it is normalized).
		/// The unit quaternion specifies a rotation that can be converted into a corresponding rotation
		/// matrix.
		/// </para>
		/// <para>
		/// The resulting 4 x 4 matrix specifies a 3-dimensional rotation in the homogeneous coordinate
		/// space. The translation part of the matrix is set to (0, 0, 0).
		/// </para>
		/// </remarks>
		public static Matrix44F ToRotationMatrix44(this Quaternion q)
		{
			Matrix44F m = new Matrix44F();

			float twoX = 2 * q.X;
			float twoY = 2 * q.Y;
			float twoZ = 2 * q.Z;
			float twoXX = twoX * q.X;
			float twoYY = twoY * q.Y;
			float twoZZ = twoZ * q.Z;
			float twoXY = twoX * q.Y;
			float twoXZ = twoX * q.Z;
			float twoYZ = twoY * q.Z;
			float twoXW = twoX * q.W;
			float twoYW = twoY * q.W;
			float twoZW = twoZ * q.W;

			// according to Watt, p.489
			m.M00 = 1 - (twoYY + twoZZ);
			m.M01 = twoXY - twoZW;
			m.M02 = twoYW + twoXZ;
			m.M03 = 0;

			m.M10 = twoXY + twoZW;
			m.M11 = 1 - (twoXX + twoZZ);
			m.M12 = twoYZ - twoXW;
			m.M13 = 0;

			m.M20 = twoXZ - twoYW;
			m.M21 = twoXW + twoYZ;
			m.M22 = 1 - (twoXX + twoYY);
			m.M23 = 0;

			m.M30 = 0;
			m.M31 = 0;
			m.M32 = 0;
			m.M33 = 1;

			return m;
		}

		/// <summary>
		/// Rotates a vector.
		/// </summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The rotated vector.</returns>
		/// <remarks>
		/// <para>
		/// The rotation of a vector <i>v</i> by quaternion <i>q</i> is defined as:
		/// </para>
		/// <para>
		/// <i>(0, <i>v'</i>)</i> = <i>q</i> * (0, <i>v</i>) * <i>q</i><sup>-1</sup>
		/// </para>
		/// </remarks>
		public static Vector3 Rotate(this Quaternion q, Vector3 vector)
		{
			// ----- Matrix version
			// return RotationMatrix33 * vector;

			// ----- Quaternion algebra version
			//return (this * new Quaternion(0, vector) * Inverse).V;

			// ----- Optimized version #1
			// The W component of the vector is zero, so we can simplify the first 
			// quaternion multiplication. And, we do not need to compute the final w
			// component.

			// First multiplication: 
			//   this * new Quaternion(0, vector)

			//float w = -Vector3F.Dot(V, vector);

			//Vector3F v = Vector3F.Cross(V, vector) + W * vector;

			//// Second multiplication (vector component only): 
			////   q * Inverse
			//Vector3F inverse;
			//inverse.X = -X;
			//inverse.Y = -Y;
			//inverse.Z = -Z;

			//v = Vector3F.Cross(v, inverse) + w * inverse + W * v;
			//return v;

			// ----- Optimized version #2
			// Derivation by Fabian Giesen on Molly Rocket forum.
			// See http://mollyrocket.com/forums/molly_forum_833.html

			// t = 2 * cross(q.xyz, v)
			// v' = v + q.w * t + cross(q.xyz, t)
			float qX = q.X;
			float qY = q.Y;
			float qZ = q.Z;
			float qW = q.W;
			float tX = 2 * (qY * vector.Z - qZ * vector.Y);
			float tY = 2 * (qZ * vector.X - qX * vector.Z);
			float tZ = 2 * (qX * vector.Y - qY * vector.X);
			return new Vector3(
				vector.X + qW * tX + (qY * tZ - qZ * tY),
				vector.Y + qW * tY + (qZ * tX - qX * tZ),
				vector.Z + qW * tZ + (qX * tY - qY * tX));
		}

		/// <summary>
		/// Gets or sets the angle of the rotation around <see cref="Axis"/>.
		/// </summary>
		/// <value>The angle in radians.</value>
		/// <remarks>
		/// <para>
		/// Setting the angle influences all components of the quaternion. The result is a unit
		/// quaternion that specifies a rotation of <i>angle</i> radians around the axis given by 
		/// <see cref="Axis"/>.
		/// </para>
		/// <para>
		/// This property assumes that the quaternion is a unit quaternion. It returns
		/// <see cref="Double.NaN"/> if the w component is numerically greater than 1.0 or less than
		/// -1.0.
		/// </para>
		/// </remarks>
		public static float Angle(this Quaternion q)
		{
			float w = q.W;

			// Return NaN if w is not in [-1, 1] (with numerical tolerance).
			if (Numeric.IsGreater(Math.Abs(w), 1))
				return float.NaN;

			// Clamp to allowed range.
			w = MathHelper.Clamp(w, -1, 1);

			return (float)Math.Acos(w) * 2;
		}

		/// <summary>
		/// Gets or sets the normalized unit vector with the direction of the rotation axis.
		/// </summary>
		/// <value>
		/// The normalized unit vector with the direction of the rotation axis.
		/// </value>
		/// <remarks>
		/// <para>
		/// Setting the axis influences all components of the quaternion. The result is a unit
		/// quaternion that specifies a rotation of <see cref="Angle"/> radians around the specified
		/// axis.
		/// </para>
		/// <para>
		/// If the quaternion represents "no rotation" (rotation angle is 0), the axis vector is 
		/// (0, 0, 0).
		/// </para>
		/// </remarks>
		public static Vector3 Axis(this Quaternion q)
		{
			var v = new Vector3(q.X, q.Y, q.Z);

			// If we can normalize v, this is our axis.
			if (v.TryNormalize())
				return v;

			// Could not normalize v. --> No rotation, no axis.
			return Vector3.Zero;
		}

		/// <summary>
		/// Creates a unit quaternion that specifies a rotation by a given angle around the x-axis.
		/// </summary>
		/// <param name="angle">The rotation angle in radians.</param>
		/// <returns>
		/// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
		/// radians around the x-axis.
		/// </returns>
		public static Quaternion CreateRotationX(float angle)
		{
			Quaternion q;
			float halfangle = angle / 2.0f;

			// W = cos(angle / 2);
			q.W = (float)Math.Cos(halfangle);

			// V = sin(angle / 2) * axis
			q.X = (float)Math.Sin(halfangle);
			q.Y = 0;
			q.Z = 0;

			return q;
		}


		/// <summary>
		/// Creates a unit quaternion that specifies a rotation by a given angle around the y-axis.
		/// </summary>
		/// <param name="angle">The rotation angle in radians.</param>
		/// <returns>
		/// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
		/// radians around the y-axis.
		/// </returns>
		public static Quaternion CreateRotationY(float angle)
		{
			Quaternion q;
			float halfangle = angle / 2.0f;

			// W = cos(angle / 2);
			q.W = (float)Math.Cos(halfangle);

			// V = sin(angle / 2) * axis
			q.X = 0;
			q.Y = (float)Math.Sin(halfangle);
			q.Z = 0;

			return q;
		}


		/// <summary>
		/// Creates a unit quaternion that specifies a rotation by a given angle around the z-axis.
		/// </summary>
		/// <param name="angle">The rotation angle in radians.</param>
		/// <returns>
		/// The created unit quaternion that describes a rotation by the <paramref name="angle"/>
		/// radians around the z-axis.
		/// </returns>
		public static Quaternion CreateRotationZ(float angle)
		{
			Quaternion q;
			float halfangle = angle / 2.0f;

			// W = cos(angle / 2);
			q.W = (float)Math.Cos(halfangle);

			// V = sin(angle / 2) * axis
			q.X = 0;
			q.Y = 0;
			q.Z = (float)Math.Sin(halfangle);

			return q;
		}

		/// <overloads>
		/// <summary>
		/// Creates a quaternion for a given rotation.
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Gets an orientation quaternion from Euler angles (3 rotations around 3 axes).
		/// </summary>
		/// <param name="angle1">The first angle.</param>
		/// <param name="axis1">The first axis.</param>
		/// <param name="angle2">The second angle.</param>
		/// <param name="axis2">The second axis.</param>
		/// <param name="angle3">The third angle.</param>
		/// <param name="axis3">The third axis.</param>
		/// <param name="useGlobalAxes">
		/// If set to <see langword="true"/> then the rotation axes are fixed in world space. Otherwise 
		/// the rotation axes are fixed on the object and rotated with each rotation.
		/// </param>
		/// <remarks>
		/// A rotation is created from 3 sequential rotations. Each rotation is defined by an angle and 
		/// the rotation axis. This method can be used to create a quaternion from Euler angle 
		/// representations, often named Azimuth/Elevation/Roll, or Heading/Pitch/Roll.
		/// </remarks>
		/// <returns>
		/// The orientation quaternion that describes the same orientation as the given Euler angles.
		/// </returns>
		/// <exception cref="MathematicsException">
		/// The length of the axis vectors must not be <c>0</c>.
		/// </exception>
		public static Quaternion CreateRotation(float angle1, Vector3 axis1, float angle2, Vector3 axis2,
			float angle3, Vector3 axis3, bool useGlobalAxes)
		{
			Quaternion rotation1 = MathHelper.CreateRotation(axis1, angle1);
			Quaternion rotation2 = MathHelper.CreateRotation(axis2, angle2);
			Quaternion rotation3 = MathHelper.CreateRotation(axis3, angle3);

			if (useGlobalAxes)
				return rotation3 * rotation2 * rotation1;
			else
				return rotation1 * rotation2 * rotation3;
		}

		/// <summary>
		/// Creates a unit quaternion that specifies a rotation given by axis and angle.
		/// </summary>
		/// <param name="axis">The axis. (Vector does not need to be normalized.)</param>
		/// <param name="angle">The angle.</param>
		/// <returns>
		/// <para>
		/// The created unit quaternion that describes a rotation by the 
		/// <paramref name="angle"/> radians around the <paramref name="axis"/>.
		/// (<paramref name="axis"/> will be normalized automatically.)
		/// </para>
		/// <para>
		/// The resulting quaternion is: <i>q</i> = (cos(<i>θ</i>/2), <i><b>v</b></i>sin(<i>θ</i>/2))
		/// </para>
		/// <para>
		/// <i>q</i> = (cos(<i>θ</i>/2), <i><b>v</b></i>sin(<i>θ</i>/2))
		/// </para>
		/// where <i>θ</i> is the angle and <i><b>v</b></i> is the normalized axis.
		/// </returns>
		/// <exception cref="ArgumentException">
		/// The <paramref name="axis"/> vector has 0 length.
		/// </exception>
		public static Quaternion CreateRotation(Vector3 axis, float angle)
		{
			if (!axis.TryNormalize())
				throw new ArgumentException("The axis vector has length 0.");

			return Quaternion.CreateFromAxisAngle(axis, angle);
		}

		/// <summary>
		/// Returns the conjugate of the quaternion.
		/// </summary>
		/// <value>The conjugate of this quaternion.</value>
		/// <remarks>
		/// <para>
		/// The conjugate of a quaternion is calculated by negating the vector component.
		/// </para>
		/// <para>
		/// <i>q<sup>*</sup> = w - <b>i</b>x - <b>j</b>y - <b>k</b>z</i>
		/// </para>
		/// <para>
		/// The property does not change this quaternion. To conjugate this instance you need to call 
		/// <see cref="Conjugate"/>.
		/// </para>
		/// </remarks>
		public static Quaternion Conjugated(this Quaternion q)
		{
			Quaternion result = q;
			result.Conjugate();
			return result;
		}

		/// <overloads>
		/// <summary>
		/// Determines whether two quaternions are equal (regarding a given tolerance).
		/// </summary>
		/// </overloads>
		/// 
		/// <summary>
		/// Tests if two quaternions are equal (within the tolerance 
		/// <see cref="Numeric.EpsilonF"/>).
		/// </summary>
		/// <param name="q1">The first quaternion.</param>
		/// <param name="q2">The second quaternion.</param>
		/// <returns>
		/// <see langword="true"/> if the quaternions are equal within the tolerance 
		/// <see cref="Numeric.EpsilonF"/>; otherwise <see langword="false"/>.
		/// </returns>
		/// <remarks>
		/// For the test the components of the quaternions are compared.
		/// </remarks>
		public static bool AreNumericallyEqual(Quaternion q1, Quaternion q2)
		{
			return Numeric.AreEqual(q1.W, q2.W)
					&& Numeric.AreEqual(q1.X, q2.X)
					&& Numeric.AreEqual(q1.Y, q2.Y)
					&& Numeric.AreEqual(q1.Z, q2.Z);
		}

		/// <summary>
		/// Calculates the angle between two quaternions.
		/// </summary>
		/// <param name="quaternion1">The first quaternion.</param>
		/// <param name="quaternion2">The second quaternion.</param>
		/// <returns>The angle between the given vectors, such that 0 ≤ angle ≤ π.</returns>
		/// <remarks>
		/// <para>
		/// The quaternions are interpreted as orientations. The result is the angle of the quaternion
		/// which would rotate an object in the first orientation to the second orientation.
		/// </para>
		/// <para>
		/// The result is only valid for unit quaternions.
		/// </para>
		/// </remarks>
		public static float GetAngle(Quaternion quaternion1, Quaternion quaternion2)
		{
			float α = Quaternion.Dot(quaternion1, quaternion2);

			// Inaccuracy in the floating-point operations can cause
			// the result be outside of the valid range.
			// Ensure that the dot product α lies in the interval [-1, 1].
			// Math.Acos() returns Double.NaN if the argument lies outside
			// of this interval.
			α = MathHelper.Clamp(α, -1.0f, 1.0f);
			if (α < 0)
				α *= -1;

			return (float)(2.0 * Math.Acos(α));
		}

		/// <summary>
		/// Calculates the power of a unit quaternion.
		/// </summary>
		/// <param name="quaternion">The quaternion.</param>
		/// <param name="t">The exponent.</param>
		/// <returns>The power of the unit quaternion.</returns>
		/// <remarks>
		/// <para>
		/// <strong>Important:</strong> This method requires that the quaternion is a unit quaternion.
		/// </para>
		/// <para>
		/// The power of quaternion is defined as:
		/// </para>
		/// <para>
		/// <i>q<sup>t</sup></i> = e<sup><i><b>u</b>tθ</i></sup> 
		///                      = cos(<i>tθ</i>) + <i><b>u</b></i>sin(<i>tθ</i>)
		/// </para>
		/// </remarks>
		public static void Power(this ref Quaternion quaternion, float t)
		{
			quaternion = Exp(Ln(quaternion) * t);
		}

		/// <summary>
		/// Gets the vector part (x, y, z).
		/// </summary>
		/// <value>The vector part (x, y, z).</value>
		public static Vector3 V(this Quaternion q)
		{
			return new Vector3(q.X, q.Y, q.Z);
		}

		public static void SetAngle(this ref Quaternion q, float angle)
		{
			q = MathHelper.CreateRotation(q.Axis(), angle);
		}

		/// <summary>
		/// Creates a unit quaternion that specifies a rotation given by two vectors.
		/// </summary>
		/// <param name="startVector">
		/// The initial vector. (Vector does not need to be normalized.)
		/// </param>
		/// <param name="rotatedVector">
		/// The rotated vector. (Vector does not need to be normalized.)
		/// </param>
		/// <returns>
		/// The created unit quaternion that would rotate <paramref name="startVector"/> to 
		/// <paramref name="rotatedVector"/>.
		/// </returns>
		/// <remarks>
		/// The quaternion is set to a rotation that would rotate vector <c>startVector</c> to the
		/// orientation of vector <c>rotatedVector</c>.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The length of the <paramref name="startVector"/> and <paramref name="rotatedVector"/> must
		/// not be <c>0</c>.
		/// </exception>
		public static Quaternion CreateRotation(Vector3 startVector, Vector3 rotatedVector)
		{
			// An optimized version, which does not handle degenerate cases, is discussed here: 
			// - Beautiful maths simplification: quaternion from two vectors
			//   http://lolengine.net/blog/2013/09/18/beautiful-maths-quaternion-from-vectors

			if (!startVector.TryNormalize())
				throw new ArgumentException("Length of the start vector must not be 0.");
			if (!rotatedVector.TryNormalize())
				throw new ArgumentException("Length of the rotated vector must not be 0.");

			Vector3 axis = Vector3.Cross(startVector, rotatedVector);
			float cosθ = Vector3.Dot(startVector, rotatedVector);
			float x, y, z, w;

			// Special case:
			// When the axes are parallel with opposite directions, then cosθ is close to -1.
			// This would cause a division by zero later on - so we make a shortcut here:
			if (Numeric.AreEqual(cosθ, -1.0f))
			{
				w = 0.0f;

				// any axis normal to startVector will do
				axis = startVector.Orthonormal1();
				x = axis.X;
				y = axis.Y;
				z = axis.Z;
				return new Quaternion(x, y, z, w);
			}

			// W needs to be cos(θ/2). This can be calculated by the following operation.
			float factor = (float)Math.Sqrt(2 * (cosθ + 1));
			w = factor / 2;

			// axis has the length of sinθ. We need to scale the axis, such that it 
			// has the length sin(θ/2). This can be done by the following operation.
			// (See "DigitalRune - Knowledge Base (Math Tricks)" for more info.)
			axis /= factor;
			x = axis.X;
			y = axis.Y;
			z = axis.Z;
			return new Quaternion(x, y, z, w);
		}


		/// <summary>
		/// Returns the normalized quaternion.
		/// </summary>
		/// <value>The normalized quaternion.</value>
		/// <remarks>
		/// The property does not change this instance. To normalize this instance you need to call 
		/// <see cref="Normalize"/>.
		/// </remarks>
		/// <exception cref="DivideByZeroException">
		/// The length of the quaternion is zero. The quaternion cannot be normalized.
		/// </exception>
		public static Quaternion Normalized(this Quaternion q)
		{
			Quaternion result = q;
			result.Normalize();
			return result;
		}

		/// <summary>
		/// Tries to normalize the quaternion.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the quaternion was normalized; otherwise, <see langword="false"/> 
		/// if the quaternion could not be normalized. (The norm is numerically zero.)
		/// </returns>
		public static bool TryNormalize(this ref Quaternion q)
		{
			float norm = q.LengthSquared();
			if (Numeric.IsZero(norm, Numeric.EpsilonFSquared))
				return false;

			float scale = (float)(1.0f / Math.Sqrt(norm));
			q.W *= scale;
			q.X *= scale;
			q.Y *= scale;
			q.Z *= scale;

			return true;
		}

		/// <summary>
		/// Returns a value indicating whether this quaternion is normalized (the <see cref="Modulus"/> 
		/// is numerically equal to 1).
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this <see cref="QuaternionF"/> is normalized; otherwise, 
		/// <see langword="false"/>.
		/// </value>
		/// <remarks>
		/// <see cref="IsNumericallyNormalized"/> compares the <see cref="Modulus"/> (length) of this 
		/// quaternion against 1.0 using the default tolerance value (see 
		/// <see cref="Numeric.EpsilonF"/>).
		/// </remarks>
		public static bool IsNumericallyNormalized(this Quaternion q)
		{
				// We compare the squared length (Norm) with 1.0f.
				return Numeric.AreEqual(q.LengthSquared(), 1.0f);
		}
	}
}

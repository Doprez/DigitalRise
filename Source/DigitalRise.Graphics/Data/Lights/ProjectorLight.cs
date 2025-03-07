// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRise.Graphics.SceneGraph;
using DigitalRise.Mathematics;
using DigitalRise.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRise.Graphics
{
  /// <summary>
  /// Represents a light that projects a texture.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A projector light is similar to a <see cref="Spotlight"/>, but is usually used to project a 
  /// texture. The light emitted is shaped like a view frustum and not like a cone. And the light 
  /// does not have the typical "spotlight falloff". 
  /// </para>
  /// <para>
  /// A <see cref="Projection"/> defines the shape of the light. Usually, a 
  /// <see cref="PerspectiveProjection"/> is used, but it is also possible to use a 
  /// <see cref="OrthographicProjection"/>.
  /// </para>
  /// <para>
  /// Projector lights have color, intensity, position, direction and range. The 
  /// <see cref="ProjectorLight"/> object defines the light properties of a projector light 
  /// positioned at the origin (0, 0, 0) that shines in forward direction (0, 0, -1) - see 
  /// <see cref="Vector3.Forward"/>. A <see cref="LightNode"/> needs to be created to position and 
  /// orient a projector light within a 3D scene.
  /// </para>
  /// <para>
  /// <see cref="Color"/>, <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/>, 
  /// <see cref="HdrScale"/>, and the light distance attenuation factor (see 
  /// <see cref="GraphicsHelper.GetDistanceAttenuation"/>) are multiplied to get the final diffuse 
  /// and specular light intensities which can be used in the lighting equations. 
  /// </para>
  /// <para>
  /// When using a low dynamic range lighting (LDR lighting) the light intensities are
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Diffuse light intensity <i>L<sub>diffuse</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>DiffuseIntensity</i>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Specular light intensity <i>L<sub>specular</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>SpecularIntensity</i>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// When using a high dynamic range lighting (HDR lighting) the light intensities are
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Diffuse light intensity <i>L<sub>diffuse</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>DiffuseIntensity</i> · <i>HdrScale</i>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Specular light intensity <i>L<sub>specular</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>SpecularIntensity</i> · <i>HdrScale</i>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// A 2D texture (see <see cref="Texture"/>) must be assigned to the projector light. If a 
  /// texture is set, the light intensity is modulated with the texture to project the texture 
  /// onto the lit surroundings. By default no texture is assigned. If no texture is set, the
  /// projector light does not emit any light.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When the <see cref="ProjectorLight"/> is cloned the 
  /// <see cref="Texture"/> is not duplicated. The <see cref="Texture"/> is copied by reference.
  /// </para>
  /// </remarks>
  public class ProjectorLight : Light
  {
    // Notes: 
    // The Projection.Far replaces the Range of the normal spotlight.
    // We cannot set a default texture because the ProjectorLight is not yet assigned 
    // to a graphics device.
    //
    // A ProjectorLight with an orthographic projection is similar to a DirectionalLight -
    // but it is not the same: A directional light usually uses cascades and fits the
    // frustums to the camera. A directional light does not have a distance attenuation.
    // A ProjectorLight with a perspective projection is similar to a Spotlight -
    // but it is not the same: The projector does not use a cone attenuation and it can
    // use off-center projections.
    //
    // What if we want a projector light with no distance attenuation?
    //   We could set a very high AttenuationExponent - this creates a very small falloff zone.
    //   We could create a special technique in the light shader which ignores attenuation.
    //   This technique could be automatically used when AttenuationExponent has a special value.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the RGB color of the light.
    /// </summary>
    /// <value>The color of the light. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This property defines only the RGB color of the light source - not its intensity. 
    /// </remarks>
    public Vector3 Color { get; set; }


    /// <summary>
    /// Gets or sets the diffuse intensity of the light.
    /// </summary>
    /// <value>The diffuse intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <see cref="Color"/> and <see cref="DiffuseIntensity"/> are separate properties so the values 
    /// can be adjusted independently.
    /// </remarks>
    public float DiffuseIntensity { get; set; }


    /// <summary>
    /// Gets or sets the specular intensity of the light.
    /// </summary>
    /// <value>The specular intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <see cref="Color"/> and <see cref="SpecularIntensity"/> are separate properties so the 
    /// values can be adjusted independently.
    /// </remarks>
    public float SpecularIntensity { get; set; }


    /// <summary>
    /// Gets or sets the HDR scale of the light.
    /// </summary>
    /// <value>The HDR scale of the light. The default value is 1.</value>
    /// <remarks>
    /// The <see cref="HdrScale"/> is an additional intensity factor. The factor is applied to the 
    /// <see cref="Color"/> and <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/> when 
    /// high dynamic range lighting (HDR lighting) is enabled.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float HdrScale { get; set; }

    
    /// <summary>
    /// Gets or sets the projection.
    /// </summary>
    /// <value>
    /// The projection of the projector. The default value is a new 
    /// <see cref="PerspectiveProjection"/>.
    /// </value>
    /// <remarks>
    /// The intensity of the light continually decreases from the light's origin up to the 
    /// far distance specified in the projection. At the far plane the light intensity is 0.
    /// <see cref="Attenuation"/> the shape of the attenuation curve. See also 
    /// <see cref="GraphicsHelper.GetDistanceAttenuation"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Projection Projection
    {
      get { return _projection; }
      set 
      {
        if (value == null)
          throw new ArgumentNullException("value");
        
        _projection = value;
        Shape = _projection.ViewVolume;
      } 
    }
    private Projection _projection;


    /// <summary>
    /// Gets or sets the attenuation exponent for the distance attenuation.
    /// </summary>
    /// <value>The attenuation exponent. The default value is 2.</value>
    /// <remarks>
    /// This exponent defines the shape of the distance attenuation curve. See also
    /// <see cref="GraphicsHelper.GetDistanceAttenuation"/>.
    /// </remarks>
    public float Attenuation { get; set; }


    /// <summary>
    /// Gets or sets the texture.
    /// </summary>
    /// <value>The texture.</value>
    public Texture2D Texture { get; set; } 
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectorLight"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectorLight"/> class.
    /// </summary>
    public ProjectorLight()
      : this(null, GetDefaultProjection())
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectorLight"/> class.
    /// </summary>
    /// <param name="texture">The texture that is projected.</param>
    /// <param name="projection">The projection.</param>
    public ProjectorLight(Texture2D texture, Projection projection)
    {
      Texture = texture;
      Color = Vector3.One;
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
      Projection = projection; // Automatically sets Shape.
      Attenuation = 2;
    }


    private static PerspectiveProjection GetDefaultProjection()
    {
      // The default value of the PerspectiveProjection are too big for typical 
      // projector lights. Therefore, we use our own default value.
      const float near = 0.25f;
      const float far = 5.0f;
      const float aspectRatio = 4.0f / 3.0f;
      const float fieldOfViewY = 45.0f * ConstantsF.Pi / 180.0f; // 45°

      var projection = new PerspectiveProjection();
      projection.SetFieldOfView(fieldOfViewY, aspectRatio, near, far);
      return projection;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new ProjectorLight(null, Projection.Clone());
    }


    /// <inheritdoc/>
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone ProjectorLight properties.
      var sourceTyped = (ProjectorLight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      Texture = sourceTyped.Texture;
      Attenuation = sourceTyped.Attenuation;

      // Shape does not need to be cloned. It is automatically set by the Projection property.
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3 GetIntensity(float distance)
    {
      if (Texture == null)
        return Vector3.Zero;

      float attenuation = GraphicsHelper.GetDistanceAttenuation(distance, Projection.Far, Attenuation);
      return Vector3.Max(Color * (DiffuseIntensity * HdrScale * attenuation),
                          Color * (SpecularIntensity * HdrScale * attenuation));
    }
    #endregion
  }
}

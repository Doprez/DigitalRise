// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace DigitalRise.Input
{
  /// <summary>
  /// Manages user input from several devices. See <see cref="IInputService"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Update"/> must be called once per frame. The method must be called on the main game
  /// thread that is allowed to query the input devices (other threads are not allowed to query 
  /// input devices).
  /// </para>
  /// <para>
  /// See <see cref="IInputService"/> for additional information.
  /// </para>
  /// </remarks>
  /// <seealso cref="IInputService"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Not critical. Accelerometer implementation will be replaced in next version.")]
  public partial class InputManager : IInputService
  {
    // Notes:
    // ThumbstickThreshold and TriggerThreshold are only applied if IsDown() is called, 
    // but not if GamePadState.IsButtonDown/IsButtonUp is used directly.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Info for the last key/button press to detect double-clicks and key/button repetition.
    private class LastButtonInfo<T>
    {
      public T Button;
      public TimeSpan DownDuration;
      public TimeSpan TimeSinceLastClick = TimeSpan.MaxValue;
      public bool IsDoubleClick;
      public bool IsVirtualPress;
    }

    private class LastMouseButtonInfo<T> : LastButtonInfo<T>
    {
      public Vector2 MouseClickPosition;
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // Arrays containing all keys and buttons so that we can enumerate them easily.
    private static readonly Keys[] _keys;
    private static readonly Buttons[] _gamePadButtons;
    private static readonly MouseButtons[] _mouseButtons;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly bool _gamerServicesEnabled;

    private readonly LastMouseButtonInfo<MouseButtons> _lastMouseButton;
    private readonly LastButtonInfo<Keys> _lastKey;
    private readonly LastButtonInfo<Buttons>[] _lastGamePadButtons;

    private readonly PlayerIndex?[] _logicalPlayers;
    private readonly bool[] _areGamePadsHandled;   // Index is PlayerIndex (not LogicalPlayerIndex).
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public InputSettings Settings
    {
      get { return _settings; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _settings = value;
      }
    }
    private InputSettings _settings;


    /// <inheritdoc/>
    public int MaxNumberOfPlayers { get; private set; }


    /// <inheritdoc/>
    public bool IsAccelerometerHandled { get; set; }


    /// <inheritdoc/>
    public bool IsKeyboardHandled { get; set; }


    /// <inheritdoc/>
    public bool IsMouseOrTouchHandled { get; set; }


    /// <inheritdoc/>
    public InputCommandCollection Commands { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="InputManager"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static InputManager()
    {
      // ----- Store enum values in arrays for easy enumeration.

      // Keyboard keys:
      var keysValues = EnumHelper.GetValues(typeof(Keys));
      var maxNumberOfKeys = keysValues.Length;
      _keys = new Keys[maxNumberOfKeys];
      for (int i = 0; i < maxNumberOfKeys; i++)
        _keys[i] = (Keys)keysValues[i];

      // Gamepad buttons:
      var buttonsValues = EnumHelper.GetValues(typeof(Buttons));
      var maxNumberOfButtons = buttonsValues.Length;
      _gamePadButtons = new Buttons[maxNumberOfButtons];
      for (int i = 0; i < maxNumberOfButtons; i++)
        _gamePadButtons[i] = (Buttons)buttonsValues[i];

      // Mouse buttons:
      var mouseButtonsValues = EnumHelper.GetValues(typeof(MouseButtons));
      var maxNumberOfMouseButtons = mouseButtonsValues.Length;
      _mouseButtons = new MouseButtons[maxNumberOfMouseButtons];
      for (int i = 0; i < maxNumberOfMouseButtons; i++)
        _mouseButtons[i] = (MouseButtons)mouseButtonsValues[i];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputManager"/> class.
    /// </summary>
    /// <param name="gamerServicesEnabled">
    /// <see langword="true"/> if the game uses the XNA gamer services component; otherwise, 
    /// <see langword="false"/>.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "gamerServicesEnabled", Justification = "The parameter should be removed when we stop supporting XNA.")]
    public InputManager(bool gamerServicesEnabled)
    {
      _gamerServicesEnabled = gamerServicesEnabled;

      MaxNumberOfPlayers = 4;
      int gamePadArrayLength = 4;

      // Create default settings.
      Settings = new InputSettings();

      // Create map: logical player --> game controller.
      _logicalPlayers = new PlayerIndex?[gamePadArrayLength];

      // Create last-button-info that stores the last pressed button for double-click detection
      // and button repetition (virtual button presses).
      _lastKey = new LastButtonInfo<Keys>();
      _lastMouseButton = new LastMouseButtonInfo<MouseButtons>();
      _lastGamePadButtons = new LastButtonInfo<Buttons>[gamePadArrayLength];
      for (int i = 0; i < _lastGamePadButtons.Length; i++)
        _lastGamePadButtons[i] = new LastButtonInfo<Buttons>();

      // Keyboard
      _pressedKeys = new List<Keys>(10);                // Initial capacity: 10 (fingers)
      _pressedKeysAsReadOnly = new ReadOnlyCollection<Keys>(_pressedKeys); // Public read-only wrapper


      // Gamepads
      _areGamePadsHandled = new bool[gamePadArrayLength];
      _previousGamePadStates = new GamePadState[gamePadArrayLength];
      _newGamePadStates = new GamePadState[gamePadArrayLength];

      // Touch
      try
      {
        TouchPanel.EnabledGestures = GestureType.None;
      }
      catch (NullReferenceException exception)
      {
        // This happens in UWP interop projects. Mouse/touch/keyboard do not work in this case.
        throw new NotSupportedException(
          "MonoGame threw an exception. Please note that the DigitalRise InputManager and the " +
          "MonoGame mouse/touch/keyboard classes cannot be used in some interop scenarios.", 
          exception);
      }

      _gestures = new List<GestureSample>();

      // Input commands
      Commands = new InputCommandCollection(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void SetGamePadHandled(LogicalPlayerIndex player, bool value)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        foreach (PlayerIndex? controller in _logicalPlayers)
        {
          if (controller.HasValue)
            _areGamePadsHandled[(int)controller.Value] = value;
        }
      }
      else
      {
        var controller = GetLogicalPlayer(player);
        if (controller.HasValue)
          _areGamePadsHandled[(int)controller.Value] = value;
      }
    }


    /// <inheritdoc/>
    public void SetGamePadHandled(PlayerIndex controller, bool value)
    {
      int index = (int)controller;
      _areGamePadsHandled[index] = value;
    }


    /// <inheritdoc/>
    public bool IsGamePadHandled(LogicalPlayerIndex player)
    {
      if (player == LogicalPlayerIndex.Any)
      {
        foreach (var controller in _logicalPlayers)
        {
          if (controller.HasValue && _areGamePadsHandled[(int)controller.Value])
            return true;
        }
      }
      else
      {
        var controller = GetLogicalPlayer(player);
        if (controller.HasValue)
          return _areGamePadsHandled[(int)controller.Value];
      }

      return false;
    }


    /// <inheritdoc/>
    public bool IsGamePadHandled(PlayerIndex controller)
    {
      int index = (int)controller;
      return _areGamePadsHandled[index];
    }

    /// <inheritdoc/>
    public void SetAllHandled(bool value)
    {
      IsAccelerometerHandled = value;
      IsKeyboardHandled = value;
      IsMouseOrTouchHandled = value;
      for (int i = 0; i < _areGamePadsHandled.Length; i++)
        _areGamePadsHandled[i] = value;
    }


    /// <summary>
    /// Updates the input states. This method must be called once per frame.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      // Reset IsHandled flags.
      // If the XNA guide or MonoGame guide replacement is visible, we have to ignore input.
      bool isHandled = (_gamerServicesEnabled);

      IsAccelerometerHandled = isHandled;
      IsKeyboardHandled = isHandled;
      IsMouseOrTouchHandled = isHandled;
      for (int i = 0; i < _areGamePadsHandled.Length; i++)
        _areGamePadsHandled[i] = isHandled;

      // Update input devices.
      UpdateKeyboard(deltaTime);
      UpdateMouse(deltaTime);
      UpdateGamePads(deltaTime);
      UpdateTouch(deltaTime);

      // Update commands.
      foreach (var command in Commands)
        command.Update(deltaTime);
    }
    #endregion
  }
}

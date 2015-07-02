﻿/*
 * This file is part of NScumm.
 * 
 * NScumm is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * NScumm is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with NScumm.  If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.Xna.Framework.Input;
using NScumm.Core.Input;
using System.Collections.Generic;
using NScumm.Core;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
#if WINDOWS_UAP
using Windows.UI.Core;
#endif

namespace NScumm.MonoGame
{
    sealed class XnaInputManager : IInputManager
    {
        MouseState _mouseState;
        KeyboardState _keyboardState;
        TouchPanelState _touchState;
        double _width;
        double _height;
        GameWindow _window;
        readonly object _gate = new object();
        bool _backPressed;
        bool _leftButtonPressed;
        bool _isMenuPressed;
        Core.Graphics.Point _mousePosition;
        private bool _rightButtonPressed;
        private Vector2 _realPosition;

        public Vector2 RealPosition { get { return _realPosition; } }

        public XnaInputManager(GameWindow window, int width, int height)
        {
            _window = window;
            _width = width;
            _height = height;
            _mousePosition = new Core.Graphics.Point();

#if WINDOWS_UAP
            var view = SystemNavigationManager.GetForCurrentView();
            view.BackRequested += HardwareButtons_BackPressed;

            var touchCap = TouchPanel.GetCapabilities();
            TouchPanel.EnableMouseTouchPoint = true;
            TouchPanel.EnableMouseGestures = true;
            TouchPanel.EnabledGestures = GestureType.Hold | GestureType.Tap;

            // note, cache the value instead of querying it more than once.
            bool isHardwareButtonsAPIPresent = Windows.Foundation.Metadata.ApiInformation.IsTypePresent
                (typeof(Windows.Phone.UI.Input.HardwareButtons).FullName);

            if (isHardwareButtonsAPIPresent)
            {
                Windows.Phone.UI.Input.HardwareButtons.CameraPressed +=
                    this.HardwareButtons_CameraPressed;
            }
#endif
        }

#if WINDOWS_UAP
        private void HardwareButtons_BackPressed(object sender, BackRequestedEventArgs e)
        {
            _backPressed = true;
            e.Handled = true;
        }

        private void HardwareButtons_CameraPressed(object sender, Windows.Phone.UI.Input.CameraEventArgs e)
        {
            _isMenuPressed = true;
        }
#endif
        public Core.Graphics.Point GetMousePosition()
        {
            _leftButtonPressed = false;
            _rightButtonPressed = false;

#if WINDOWS_UAP
            if (_touchState != null)
            {
                if (TouchPanel.IsGestureAvailable)
                {
                    var gesture = _touchState.ReadGesture();
                    if (gesture.GestureType == GestureType.Tap)
                    {
                        var pos = gesture.Position;
                        UpdateMousePosition(pos.X, pos.Y);
                        _leftButtonPressed = true;
                    }
                    else if (gesture.GestureType == GestureType.Hold)
                    {
                        var pos = gesture.Position;
                        UpdateMousePosition(pos.X, pos.Y);
                        _rightButtonPressed = true;
                    }
                }

                var locations = _touchState.GetState();
                foreach (var touch in locations)
                {
                    if (touch.State == TouchLocationState.Moved || touch.State == TouchLocationState.Pressed)
                    {
                        var pos = locations[0].Position;
                        UpdateMousePosition(pos.X, pos.Y);
                        _leftButtonPressed = true;
                    }
                }
            }
#else
            {
                var state = _mouseState;
                var x = state.X;
                var y = state.Y;
                UpdateMousePosition(x, y);
            }
#endif
            return _mousePosition;
        }

        private void UpdateMousePosition(float x, float y)
        {
            var scaleX = _width / _window.ClientBounds.Width;
            var scaleY = _height / _window.ClientBounds.Height;
            _realPosition = new Vector2(x, y);
            _mousePosition = new Core.Graphics.Point((int)(x * scaleX), (int)(y * scaleY));
        }

        public void UpdateInput(MouseState mouse, KeyboardState keyboard)
        {
            lock (_gate)
            {
                _mouseState = mouse;
                _keyboardState = keyboard;
                _touchState = TouchPanel.GetState(_window);
            }
        }

        public ScummInputState GetState()
        {
            lock (_gate)
            {
                var keys = _keyboardState.GetPressedKeys().Where(keyToKeyCode.ContainsKey).Select(key => keyToKeyCode[key]).ToList();
                if (_backPressed)
                {
                    keys.Add(KeyCode.Escape);
                    _backPressed = false;
                }
                if (_isMenuPressed)
                {
                    keys.Add(KeyCode.F5);
                    _isMenuPressed = false;
                }
                var inputState = new ScummInputState(keys, _leftButtonPressed || _mouseState.LeftButton == ButtonState.Pressed, _rightButtonPressed || _mouseState.RightButton == ButtonState.Pressed);
                return inputState;
            }
        }

        static readonly Dictionary<Keys, KeyCode> keyToKeyCode = new Dictionary<Keys, KeyCode>
        {
            { Keys.Back,   KeyCode.Backspace },
            { Keys.Tab,    KeyCode.Tab       },
            { Keys.Enter,  KeyCode.Return    },
            { Keys.Escape, KeyCode.Escape    },
            { Keys.Space,  KeyCode.Space     },
            { Keys.F1 , KeyCode.F1 },
            { Keys.F2 , KeyCode.F2 },
            { Keys.F3 , KeyCode.F3 },
            { Keys.F4 , KeyCode.F4 },
            { Keys.F5 , KeyCode.F5 },
            { Keys.F6 , KeyCode.F6 },
            { Keys.F7 , KeyCode.F7 },
            { Keys.F8 , KeyCode.F8 },
            { Keys.F9 , KeyCode.F9 },
            { Keys.F10, KeyCode.F10 },
            { Keys.F11, KeyCode.F11 },
            { Keys.F12, KeyCode.F12 },
            { Keys.A, KeyCode.A },
            { Keys.B, KeyCode.B },
            { Keys.C, KeyCode.C },
            { Keys.D, KeyCode.D },
            { Keys.E, KeyCode.E },
            { Keys.F, KeyCode.F },
            { Keys.G, KeyCode.G },
            { Keys.H, KeyCode.H },
            { Keys.I, KeyCode.I },
            { Keys.J, KeyCode.J },
            { Keys.K, KeyCode.K },
            { Keys.L, KeyCode.L },
            { Keys.M, KeyCode.M },
            { Keys.N, KeyCode.N },
            { Keys.O, KeyCode.O },
            { Keys.P, KeyCode.P },
            { Keys.Q, KeyCode.Q },
            { Keys.R, KeyCode.R },
            { Keys.S, KeyCode.S },
            { Keys.T, KeyCode.T },
            { Keys.U, KeyCode.U },
            { Keys.V, KeyCode.V },
            { Keys.W, KeyCode.W },
            { Keys.X, KeyCode.X },
            { Keys.Y, KeyCode.Y },
            { Keys.Z, KeyCode.Z },
            { Keys.D0, KeyCode.D0 },
            { Keys.D1, KeyCode.D1 },
            { Keys.D2, KeyCode.D2 },
            { Keys.D3, KeyCode.D3 },
            { Keys.D4, KeyCode.D4 },
            { Keys.D5, KeyCode.D5 },
            { Keys.D6, KeyCode.D6 },
            { Keys.D7, KeyCode.D7 },
            { Keys.D8, KeyCode.D8 },
            { Keys.D9, KeyCode.D9 },
            { Keys.NumPad0, KeyCode.NumPad0 },
            { Keys.NumPad1, KeyCode.NumPad1 },
            { Keys.NumPad2, KeyCode.NumPad2 },
            { Keys.NumPad3, KeyCode.NumPad3 },
            { Keys.NumPad4, KeyCode.NumPad4 },
            { Keys.NumPad5, KeyCode.NumPad5 },
            { Keys.NumPad6, KeyCode.NumPad6 },
            { Keys.NumPad7, KeyCode.NumPad7 },
            { Keys.NumPad8, KeyCode.NumPad8 },
            { Keys.NumPad9, KeyCode.NumPad9 },
            { Keys.Up, KeyCode.Up },
            { Keys.Down, KeyCode.Down },
            { Keys.Right, KeyCode.Right },
            { Keys.Left, KeyCode.Left },
            { Keys.Insert, KeyCode.Insert },
            { Keys.Home, KeyCode.Home },
            { Keys.End, KeyCode.End },
            { Keys.PageUp, KeyCode.PageUp },
            { Keys.PageDown, KeyCode.PageDown },
            { Keys.LeftShift, KeyCode.LeftShift },
        };        
    }
}
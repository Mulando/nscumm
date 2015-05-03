﻿//
//  ScummEngine_Variables.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Collections;
using NScumm.Core.IO;

namespace NScumm.Core
{
    partial class ScummEngine
    {
        public int? VariableEgo;
        public int? VariableCameraPosX;
        public int? VariableCameraPosY;
        public int? VariableHaveMessage;
        public int? VariableRoom;
        public int? VariableOverride;
        public int? VariableCurrentLights;
        public int? VariableTimer1;
        public int? VariableTimer2;
        public int? VariableTimer3;
        public int? VariableMusicTimer;
        public int? VariableCameraMinY;
        public int? VariableCameraMaxY;
        public int? VariableCameraMinX;
        public int? VariableCameraMaxX;
        public int? VariableTimerNext;
        public int? VariableVirtualMouseX;
        public int? VariableVirtualMouseY;
        public int? VariableRoomResource;
        public int? VariableLastSound;
        public int? VariableCutSceneExitKey;
        public int? VariableTalkActor;
        public int? VariableCameraFastX;
        public int? VariableScrollScript;
        public int? VariableEntryScript;
        public int? VariableEntryScript2;
        public int? VariableExitScript;
        public int? VariableExitScript2;
        public int? VariableVerbScript;
        public int? VariableSentenceScript;
        public int? VariableInventoryScript;
        public int? VariableCutSceneStartScript;
        public int? VariableCutSceneEndScript;
        public int? VariableCharIncrement;
        public int? VariableWalkToObject;
        public int? VariableDebugMode;
        public int? VariableHeapSpace;
        public int? VariableMouseX;
        public int? VariableMouseY;
        public int? VariableTimer;
        public int? VariableTimerTotal;
        public int? VariableSoundcard;
        public int? VariableVideoMode;
        public int? VariableMainMenu;
        public int? VariableFixedDisk;
        public int? VariableCursorState;
        public int? VariableUserPut;
        public int? VariableTalkStringY;
        public int? VariableNoSubtitles;
        public int? VariableSoundResult;
        public int? VariableTalkStopKey;
        public int? VariableFadeDelay;
        public int? VariableSoundParam;
        public int? VariableSoundParam2;
        public int? VariableSoundParam3;
        public int? VariableInputMode;
        public int? VariableMemoryPerformance;
        public int? VariableVideoPerformance;
        public int? VariableRoomFlag;
        public int? VariableGameLoaded;
        public int? VariableNewRoom;
        public int? VariableRoomWidth;
        public int? VariableRoomHeight;
        public int? VariableVoiceMode;
        public int? VariableSaveLoadScript;
        public int? VariableSaveLoadScript2;
        public int? VariableLeftButtonHold;
        public int? VariableRightButtonHold;
        public int? VariableLeftButtonDown;
        public int? VariableRightButtonDown;
        public int? VariableV6SoundMode;
        public int? VariableV6EMSSpace;
        public int? VariableCameraThresholdX;
        public int? VariableCameraThresholdY;
        public int? VariableCameraAccelX;
        public int? VariableCameraAccelY;
        public int? VariableVoiceBundleLoaded;
        public int? VariableDefaultTalkDelay;
        public int? VariableMusicBundleLoaded;
        public int? VariableCurrentDisk;
        public int? VariableActiveVerb;
        public int? VariableActiveObject1;
        public int? VariableActiveObject2;
        public int? VariableVerbAllowed;
        public int? VariableCharCount;

        int[] _variables;
        protected BitArray _bitVars;
        protected Stack<int> _stack = new Stack<int>();
        protected int _resultVarIndex;

        public int[] Variables
        {
            get { return _variables; }
        }

        protected byte ReadByte()
        {
            return _currentScriptData[_currentPos++];
        }

        protected virtual uint ReadWord()
        {
            ushort word = (ushort)(_currentScriptData[_currentPos++] | (_currentScriptData[_currentPos++] << 8));
            return word;
        }

        protected virtual void GetResult()
        {
            _resultVarIndex = (int)ReadWord();
            if ((_resultVarIndex & 0x2000) == 0x2000)
            {
                var a = ReadWord();
                if ((a & 0x2000) == 0x2000)
                {
                    _resultVarIndex += ReadVariable((uint)(a & ~0x2000));
                }
                else
                {
                    _resultVarIndex += (int)(a & 0xFFF);
                }
                _resultVarIndex &= ~0x2000;
            }
        }

        protected virtual int ReadVariable(uint var)
        {
            if (((var & 0x2000) != 0) && (Game.Version <= 5))
            {
                var a = ReadWord();
                if ((a & 0x2000) == 0x2000)
                    var += (uint)ReadVariable((uint)(a & ~0x2000));
                else
                    var += a & 0xFFF;
                var = (uint)(var & ~0x2000);
            }

            if ((var & 0xF000) == 0)
            {
//                Debug.WriteLine("ReadVariable({0}) => {1}", var, _variables[var]);
                ScummHelper.AssertRange(0, var, _resManager.NumVariables - 1, "variable (reading)");
                if (var == 490 && _game.GameId == GameId.Monkey2)
                {
                    var = 518;
                }
                return _variables[var];
            }

            if ((var & 0x8000) == 0x8000)
            {
//                Debug.Write(string.Format("ReadVariable({0}) => ", var));
                if (_game.Version <= 3)
                {
                    int bit = (int)(var & 0xF);
                    var = (var >> 4) & 0xFF;

                    ScummHelper.AssertRange(0, var, _resManager.NumVariables - 1, "variable (reading)");
                    return (_variables[var] & (1 << bit)) > 0 ? 1 : 0;
                }
                var &= 0x7FFF;

                ScummHelper.AssertRange(0, var, _bitVars.Length - 1, "variable (reading)");
//                Debug.WriteLine(_bitVars[var]);
                return _bitVars[(int)var] ? 1 : 0;
            }

            if ((var & 0x4000) == 0x4000)
            {
//                Debug.Write(string.Format("ReadVariable({0}) => ", var));
                if (Game.Features.HasFlag(GameFeatures.FewLocals))
                {
                    var &= 0xF;
                }
                else
                {
                    var &= 0xFFF;
                }

                ScummHelper.AssertRange(0, var, 20, "local variable (reading)");
//                Debug.WriteLine(_slots[_currentScript].LocalVariables[var]);
                return _slots[_currentScript].LocalVariables[var];
            }

            throw new NotSupportedException("Illegal varbits (r)");
        }

        protected int GetVarOrDirectWord(OpCodeParameter param)
        {
            if (((OpCodeParameter)_opCode).HasFlag(param))
                return GetVar();
            return ReadWordSigned();
        }

        protected int GetVarOrDirectByte(OpCodeParameter param)
        {
            if (((OpCodeParameter)_opCode).HasFlag(param))
                return GetVar();
            return ReadByte();
        }

        protected virtual int GetVar()
        {
            return ReadVariable(ReadWord());
        }

        protected virtual int ReadWordSigned()
        {
            return (short)ReadWord();
        }

        protected int[] GetWordVarArgs()
        {
            var args = new List<int>();
            while ((_opCode = ReadByte()) != 0xFF)
            {
                args.Add(GetVarOrDirectWord(OpCodeParameter.Param1));
            }
            return args.ToArray();
        }

        protected void SetResult(int value)
        {
            WriteVariable((uint)_resultVarIndex, value);
        }

        protected virtual void WriteVariable(uint index, int value)
        {
            //            Console.WriteLine("SetResult({0},{1})", index, value);
            if ((index & 0xF000) == 0)
            {
                ScummHelper.AssertRange(0, index, _resManager.NumVariables - 1, "variable (writing)");
                _variables[index] = value;
                return;
            }

            if ((index & 0x8000) != 0)
            {
                if (_game.Version <= 3)
                {
                    var bit = (int)(index & 0xF);
                    index = (index >> 4) & 0xFF;
                    ScummHelper.AssertRange(0, index, _resManager.NumVariables - 1, "variable (writing)");
                    if (value > 0)
                        _variables[index] |= (1 << bit);
                    else
                        _variables[index] &= ~(1 << bit);
                }
                else
                {
                    index &= 0x7FFF;

                    ScummHelper.AssertRange(0, index, _bitVars.Length - 1, "bit variable (writing)");
                    _bitVars[(int)index] = value != 0;
                }
                return;
            }

            if ((index & 0x4000) != 0)
            {
                if (Game.Features.HasFlag(GameFeatures.FewLocals))
                {
                    index &= 0xF;
                }
                else
                {
                    index &= 0xFFF;
                }

                ScummHelper.AssertRange(0, index, 20, "local variable (writing)");
                //Console.WriteLine ("SetLocalVariables(script={0},var={1},value={2})", _currentScript, index, value);
                _slots[_currentScript].LocalVariables[index] = value;
                return;
            }
        }

        void SetVarRange()
        {
            GetResult();
            var a = ReadByte();
            int b;
            do
            {
                if ((_opCode & 0x80) == 0x80)
                    b = ReadWordSigned();
                else
                    b = ReadByte();
                SetResult(b);
                _resultVarIndex++;
            } while ((--a) > 0);
        }

        void UpdateVariables()
        {
            if (Game.Version >= 7)
            {
                Variables[VariableCameraPosX.Value] = Camera.CurrentPosition.X;
                Variables[VariableCameraPosY.Value] = Camera.CurrentPosition.Y;
            }
            else
            {
                _variables[VariableCameraPosX.Value] = _camera.CurrentPosition.X;
            }
            if (Game.Version <= 7)
                Variables[VariableHaveMessage.Value] = _haveMsg;
        }
    }
}


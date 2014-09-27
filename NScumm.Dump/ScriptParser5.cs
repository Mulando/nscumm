﻿//
//  ScriptParser4.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using NScumm.Core;
using System.Collections.Generic;
using NScumm.Core.IO;

namespace NScumm.Dump
{
    class ScriptParser5: ScriptParser4
    {
        public ScriptParser5(GameInfo game)
            : base(game)
        {
        }

        protected override void InitOpCodes()
        {
            base.InitOpCodes();

            opCodes[0x25] = PickupObject;
            opCodes.Remove(0x45);
            opCodes[0x65] = PickupObject;
            opCodes[0xA5] = PickupObject;
            opCodes.Remove(0xC5);
            opCodes[0xE5] = PickupObject;

            opCodes.Remove(0x50);
            opCodes.Remove(0xD0);

            opCodes.Remove(0x5C);
            opCodes.Remove(0xDC);

            opCodes[0x0F] = GetObjectState;
            opCodes.Remove(0x4F);
            opCodes[0x8F] = GetObjectState;
            opCodes.Remove(0xCF);

            opCodes.Remove(0x2F);
            opCodes.Remove(0x6F);
            opCodes.Remove(0xAF);
            opCodes.Remove(0xEF);

            //opCodes[0xA7] = Dummy;
            opCodes.Remove(0xA7);

            opCodes[0x22] = GetAnimCounter;
            opCodes[0xA2] = GetAnimCounter;

            opCodes[0x3B] = GetActorScale;
            opCodes[0x4C] = SoundKludge;
            opCodes[0xBB] = GetActorScale;
        }

        IEnumerable<Statement> GetObjectState()
        {
            var exp = GetResultIndexExpression();
            var obj = GetVarOrDirectWord(OpCodeParameter.Param1);
            yield return SetResultExpression(exp, new MemberAccess(new ElementAccess("Objects", obj), "State"));
        }

        IEnumerable<Statement> GetAnimCounter()
        {
            var exp = GetResultIndexExpression();
            var index = GetVarOrDirectByte(OpCodeParameter.Param1);
            yield return SetResultExpression(exp, new MemberAccess(new ElementAccess("Actors", index), "AnimCounter"));
        }

        protected override IEnumerable<Statement> DrawObject()
        {
            var obj = GetVarOrDirectWord(OpCodeParameter.Param1);
            _opCode = ReadByte();
            switch (_opCode & 0x1F)
            {
                case 1:
                    var xpos = GetVarOrDirectWord(OpCodeParameter.Param1);
                    var ypos = GetVarOrDirectWord(OpCodeParameter.Param2);
                    yield return new MethodInvocation("DrawObject").AddArguments(obj, xpos, ypos).ToStatement();
                    break;
                case 2:
                    var state = GetVarOrDirectWord(OpCodeParameter.Param1);
                    yield return new MethodInvocation("DrawObjectState").AddArguments(obj, state).ToStatement();
                    break;
                case 0x1F:
                    break;
            }
        }

        protected override IEnumerable<Statement> PickupObject()
        {
            var obj = GetVarOrDirectWord(OpCodeParameter.Param1);
            var room = GetVarOrDirectByte(OpCodeParameter.Param2);
            yield return new MethodInvocation("PickupObject").AddArguments(obj, room).ToStatement();
        }
    }
}


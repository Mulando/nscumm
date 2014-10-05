//
//  ScriptParser_OpCode.cs
//
//  Author:
//       Scemino <scemino74@gmail.com>
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

using System;
using System.Collections.Generic;

namespace NScumm.Dump
{
    partial class ScriptParser
    {
        protected Dictionary<int, Func<Statement>> opCodes;
        protected int _opCode;

        protected virtual void InitOpCodes()
        {
            opCodes = new Dictionary<int, Func<Statement>>();
            /*			 00 */
            opCodes[0x00] = StopObjectCode;
            opCodes[0x01] = PutActor;
            opCodes[0x02] = StartMusic;
            opCodes[0x03] = GetActorRoom;
            /*			 04 */
            opCodes[0x04] = IsGreaterEqual;
            opCodes[0x05] = DrawObject;
            opCodes[0x06] = GetActorElevation;
            opCodes[0x07] = SetState;
            /*			 08 */
            opCodes[0x08] = IsNotEqual;
            opCodes[0x09] = FaceActor;
            opCodes[0x0A] = StartScript;
            opCodes[0x0B] = GetVerbEntrypoint;
            /*			 0C */
            opCodes[0x0C] = ResourceRoutines;
            opCodes[0x0D] = WalkActorToActor;
            opCodes[0x0E] = PutActorAtObject;
            opCodes[0x0F] = IfState;
            /*			 10 */
            opCodes[0x10] = GetObjectOwner;
            opCodes[0x11] = AnimateActor;
            opCodes[0x12] = PanCameraTo;
            opCodes[0x13] = ActorOps;
            /*			 14 */
            opCodes[0x14] = Print;
            opCodes[0x15] = ActorFromPosition;
            opCodes[0x16] = GetRandomNumber;
            opCodes[0x17] = And;
            /*			 18 */
            opCodes[0x18] = JumpRelative;
            opCodes[0x19] = DoSentence;
            opCodes[0x1A] = Move;
            opCodes[0x1B] = Multiply;
            /*			 1C */
            opCodes[0x1C] = StartSound;
            opCodes[0x1D] = IfClassOfIs;
            opCodes[0x1E] = WalkActorTo;
            /*			 20 */
            opCodes[0x20] = StopMusic;
            opCodes[0x21] = PutActor;
            opCodes[0x22] = SaveLoadGame;
            opCodes[0x23] = GetActorY;
            /*			 24 */
            opCodes[0x24] = LoadRoomWithEgo;
            opCodes[0x25] = DrawObject;
            opCodes[0x26] = SetVarRange;
            opCodes[0x27] = StringOperations;
            /*			 28 */
            opCodes[0x28] = EqualZero;
            opCodes[0x29] = SetOwnerOf;
            opCodes[0x2A] = StartScript;
            opCodes[0x2B] = DelayVariable;
            /*			 2C */
            opCodes[0x2C] = CursorCommand;
            opCodes[0x2D] = PutActorInRoom;
            opCodes[0x2E] = Delay;
            opCodes[0x2F] = IfNotState;
            /*			 30 */
            opCodes[0x30] = SetBoxFlags;
            opCodes[0x31] = GetInventoryCount;
            opCodes[0x32] = SetCameraAt;
            opCodes[0x33] = RoomOps;
            /*			 34 */
            opCodes[0x34] = GetDistance;
            opCodes[0x35] = FindObject;
            opCodes[0x36] = WalkActorToObject;
            opCodes[0x37] = StartObject;
            /*			 38 */
            opCodes[0x38] = IsLessEqual;
            opCodes[0x39] = DoSentence;
            opCodes[0x3A] = Subtract;
            opCodes[0x3B] = WaitForActor;
            /*			 3C */
            opCodes[0x3C] = StopSound;
            opCodes[0x3D] = FindInventory;
            opCodes[0x3E] = WalkActorTo;
            opCodes[0x3F] = DrawBox;
            /*			 40 */
            opCodes[0x40] = CutScene;
            opCodes[0x41] = PutActor;
            opCodes[0x42] = ChainScript;
            opCodes[0x43] = GetActorX;
            /*			 44 */
            opCodes[0x44] = IsLess;
            opCodes[0x45] = DrawObject;
            opCodes[0x46] = Increment;
            opCodes[0x47] = SetState;
            /*			 48 */
            opCodes[0x48] = IsEqual;
            opCodes[0x49] = FaceActor;
            opCodes[0x4A] = StartScript;
            opCodes[0x4B] = GetVerbEntrypoint;
            /*			 4C */
            opCodes[0x4C] = WaitForSentence;
            opCodes[0x4D] = WalkActorToActor;
            opCodes[0x4E] = PutActorAtObject;
            opCodes[0x4F] = IfState;
            /*			 50 */
            opCodes[0x50] = PickupObject;
            opCodes[0x51] = AnimateActor;
            opCodes[0x52] = ActorFollowCamera;
            opCodes[0x53] = ActorOps;
            /*			 54 */
            opCodes[0x54] = SetObjectName;
            opCodes[0x55] = ActorFromPosition;
            opCodes[0x56] = GetActorMoving;
            opCodes[0x57] = Or;
            /*			 58 */
            opCodes[0x58] = BeginOverride;
            opCodes[0x59] = DoSentence;
            opCodes[0x5A] = Add;
            opCodes[0x5B] = Divide;
            /*			 5C */
            opCodes[0x5C] = RoomEffect;
            opCodes[0x5D] = SetClass;
            opCodes[0x5E] = WalkActorTo;
            opCodes[0x5F] = IsActorInBox;
            /*			 60 */
            opCodes[0x60] = FreezeScripts;
            opCodes[0x61] = PutActor;
            opCodes[0x62] = StopScript;
            opCodes[0x63] = GetActorFacing;
            /*			 64 */
            opCodes[0x64] = LoadRoomWithEgo;
            opCodes[0x65] = DrawObject;
            opCodes[0x67] = GetStringWidth;
            /*			 68 */
            opCodes[0x68] = IsScriptRunning;
            opCodes[0x69] = SetOwnerOf;
            opCodes[0x6A] = StartScript;
            opCodes[0x6B] = DebugOp;
            /*			 6C */
            opCodes[0x6C] = GetActorWidth;
            opCodes[0x6D] = PutActorInRoom;
            opCodes[0x6E] = StopObjectScript;
            opCodes[0x6F] = IfNotState;
            /*			 70 */
            opCodes[0x70] = Lights;
            opCodes[0x71] = GetActorCostume;
            opCodes[0x72] = LoadRoom;
            opCodes[0x73] = RoomOps;
            /*			 74 */
            opCodes[0x74] = GetDistance;
            opCodes[0x75] = FindObject;
            opCodes[0x76] = WalkActorToObject;
            opCodes[0x77] = StartObject;
            /*			 78 */
            opCodes[0x78] = IsGreater;
            opCodes[0x79] = DoSentence;
            opCodes[0x7A] = VerbOps;
            opCodes[0x7B] = GetActorWalkBox;
            /*			 7C */
            opCodes[0x7C] = IsSoundRunning;
            opCodes[0x7D] = FindInventory;
            opCodes[0x7E] = WalkActorTo;
            opCodes[0x7F] = DrawBox;
            /*			 80 */
            opCodes[0x80] = BreakHere;
            opCodes[0x81] = PutActor;
            opCodes[0x82] = StartMusic;
            opCodes[0x83] = GetActorRoom;
            /*			 84 */
            opCodes[0x84] = IsGreaterEqual;
            opCodes[0x85] = DrawObject;
            opCodes[0x86] = GetActorElevation;
            opCodes[0x87] = SetState;
            /*			 88 */
            opCodes[0x88] = IsNotEqual;
            opCodes[0x89] = FaceActor;
            opCodes[0x8A] = StartScript;
            opCodes[0x8B] = GetVerbEntrypoint;
            /*			 8C */
            opCodes[0x8C] = ResourceRoutines;
            opCodes[0x8D] = WalkActorToActor;
            opCodes[0x8E] = PutActorAtObject;
            opCodes[0x8F] = IfState;
            /*			 90 */
            opCodes[0x90] = GetObjectOwner;
            opCodes[0x91] = AnimateActor;
            opCodes[0x92] = PanCameraTo;
            opCodes[0x93] = ActorOps;
            /*			 94 */
            opCodes[0x94] = Print;
            opCodes[0x95] = ActorFromPosition;
            opCodes[0x96] = GetRandomNumber;
            opCodes[0x97] = And;
            /*						 98 */
            opCodes[0x98] = SystemOps;
            opCodes[0x99] = DoSentence;
            opCodes[0x9A] = Move;
            opCodes[0x9B] = Multiply;
            /*						 9C */
            opCodes[0x9C] = StartSound;
            opCodes[0x9D] = IfClassOfIs;
            opCodes[0x9E] = WalkActorTo;
            opCodes[0x9F] = IsActorInBox;
            /*						 A0 */
            opCodes[0xA0] = StopObjectCode;
            opCodes[0xA1] = PutActor;
            opCodes[0xA2] = SaveLoadGame;
            opCodes[0xA3] = GetActorY;
            /*						 A4 */
            opCodes[0xA4] = LoadRoomWithEgo;
            opCodes[0xA5] = DrawObject;
            opCodes[0xA6] = SetVarRange;
            opCodes[0xA7] = SaveLoadVars;
            /*						 A8 */
            opCodes[0xA8] = NotEqualZero;
            opCodes[0xA9] = SetOwnerOf;
            opCodes[0xAA] = StartScript;
            opCodes[0xAB] = SaveRestoreVerbs;
            /*			 AC */
            opCodes[0xAC] = ExpressionFunc;
            opCodes[0xAD] = PutActorInRoom;
            opCodes[0xAE] = Wait;
            opCodes[0xAF] = IfNotState;
            /*			 B0 */
            opCodes[0xB0] = SetBoxFlags;
            opCodes[0xB1] = GetInventoryCount;
            opCodes[0xB2] = SetCameraAt;
            opCodes[0xB3] = RoomOps;
            /*			 B4 */
            opCodes[0xB4] = GetDistance;
            opCodes[0xB5] = FindObject;
            opCodes[0xB6] = WalkActorToObject;
            opCodes[0xB7] = StartObject;
            /*			 B8 */
            opCodes[0xB8] = IsLessEqual;
            opCodes[0xB9] = DoSentence;
            opCodes[0xBA] = Subtract;
            opCodes[0xBB] = WaitForActor;
            /*			 BC */
            opCodes[0xBC] = StopSound;
            opCodes[0xBD] = FindInventory;
            opCodes[0xBE] = WalkActorTo;
            opCodes[0xBF] = DrawBox;
            /*			 C0 */
            opCodes[0xC0] = EndCutscene;
            opCodes[0xC1] = PutActor;
            opCodes[0xC2] = ChainScript;
            opCodes[0xC3] = GetActorX;
            /*			 C4 */
            opCodes[0xC4] = IsLess;
            opCodes[0xC5] = DrawObject;
            opCodes[0xC6] = Decrement;
            opCodes[0xC7] = SetState;
            /*			 C8 */
            opCodes[0xC8] = IsEqual;
            opCodes[0xC9] = FaceActor;
            opCodes[0xCA] = StartScript;
            opCodes[0xCB] = GetVerbEntrypoint;
            /*			 CC */
            opCodes[0xCC] = PseudoRoom;
            opCodes[0xCD] = WalkActorToActor;
            opCodes[0xCE] = PutActorAtObject;
            opCodes[0xCF] = IfState;
            /*			 D0 */
            opCodes[0xD0] = PickupObject;
            opCodes[0xD1] = AnimateActor;
            opCodes[0xD2] = ActorFollowCamera;
            opCodes[0xD3] = ActorOps;
            /*						 D4 */
            opCodes[0xD4] = SetObjectName;
            opCodes[0xD5] = ActorFromPosition;
            opCodes[0xD6] = GetActorMoving;
            opCodes[0xD7] = Or;
            /*						 D8 */
            opCodes[0xD8] = PrintEgo;
            opCodes[0xD9] = DoSentence;
            opCodes[0xDA] = Add;
            opCodes[0xDB] = Divide;
            /*						 DC */
            opCodes[0xDC] = RoomEffect;
            opCodes[0xDD] = SetClass;
            opCodes[0xDE] = WalkActorTo;
            /*						 E0 */
            opCodes[0xE0] = FreezeScripts;
            opCodes[0xE1] = PutActor;
            opCodes[0xE2] = StopScript;
            opCodes[0xE3] = GetActorFacing;
            /*						 E4 */
            opCodes[0xE4] = LoadRoomWithEgo;
            opCodes[0xE5] = DrawObject;
            opCodes[0xE7] = GetStringWidth;
            /*						 E8 */
            opCodes[0xE8] = IsScriptRunning;
            opCodes[0xE9] = SetOwnerOf;
            opCodes[0xEA] = StartScript;
            opCodes[0xEB] = DebugOp;
            /*			 EC */
            opCodes[0xEC] = GetActorWidth;
            opCodes[0xED] = PutActorInRoom;
            opCodes[0xEF] = IfNotState;
            /*			 F0 */
            opCodes[0xF0] = Lights;
            opCodes[0xF1] = GetActorCostume;
            opCodes[0xF2] = LoadRoom;
            opCodes[0xF3] = RoomOps;
            /*						 F4 */
            opCodes[0xF4] = GetDistance;
            opCodes[0xF5] = FindObject;
            opCodes[0xF6] = WalkActorToObject;
            opCodes[0xF7] = StartObject;
            /*						 F8 */
            opCodes[0xF8] = IsGreater;
            opCodes[0xF9] = DoSentence;
            opCodes[0xFA] = VerbOps;
            opCodes[0xFB] = GetActorWalkBox;
            /*						 FC */
            opCodes[0xFC] = IsSoundRunning;
            opCodes[0xFD] = FindInventory;
            opCodes[0xFE] = WalkActorTo;
            opCodes[0xFF] = DrawBox;
        }

        Statement ExecuteOpCode()
        {
            if (!opCodes.ContainsKey(_opCode))
                throw new NotSupportedException(string.Format("Opcode 0x{0:X2} not supported yet!", _opCode));
            var startOffset = _br.BaseStream.Position - 1;
            var statement = opCodes[_opCode]();
            var endOffset = _br.BaseStream.Position;

            statement.StartOffset = startOffset;
            statement.EndOffset = endOffset;
            return statement;
        }
    }
}


﻿//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2015 
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

using NScumm.Core.Common;
using System;
using System.Collections.Generic;

namespace NScumm.Sci.Engine
{
    enum SegmentType
    {
        INVALID = 0,
        SCRIPT = 1,
        CLONES = 2,
        LOCALS = 3,
        STACK = 4,
        // 5 used to be system strings,	now obsolete
        LISTS = 6,
        NODES = 7,
        HUNK = 8,
        DYNMEM = 9,
        // 10 used to be string fragments, now obsolete

#if ENABLE_SCI32
        ARRAY = 11,
        STRING = 12,
#endif

        MAX // For sanity checking
    }

    internal abstract class SegmentObj
    {
        private SegmentType _type;

        public SegmentType Type { get { return _type; } }

        public SegmentObj(SegmentType type)
        {
            _type = type;
        }

        /// <summary>
        /// Check whether the given offset into this memory object is valid,
        /// i.e., suitable for passing to dereference.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public abstract bool IsValidOffset(ushort offset);

        /// <summary>
        /// Dereferences a raw memory pointer.
        /// </summary>
        /// <param name="pointer">reference to dereference</param>
        /// <returns>the data block referenced</returns>
        public virtual SegmentRef Dereference(Register pointer)
        {
            throw new System.InvalidOperationException($"Error: Trying to dereference pointer {pointer} to inappropriate segment");
        }

        /// <summary>
        ///  Iterates over all references reachable from the specified object.
        ///  Used by the garbage collector.
        /// </summary>
        /// <param name="obj">object (within the current segment) to analyze</param>
        /// <returns>a list of outgoing references within the object</returns>
        /// <remarks>This function may also choose to report numbers (segment 0) as adresses</remarks>
        public virtual List<Register> ListAllOutgoingReferences(Register obj)
        {
            return new List<Register>();
        }

        /// <summary>
        /// Iterates over and reports all addresses within the segment.
        /// Used by the garbage collector.
        /// </summary>
        /// <param name="segId"></param>
        /// <returns>a list of addresses within the segment</returns>
        public virtual List<Register> ListAllDeallocatable(ushort segId)
        {
            return new List<Register>();
        }

        /// <summary>
        /// Deallocates all memory associated with the specified address.
        /// Used by the garbage collector.
        /// </summary>
        /// <param name="segMan"></param>
        /// <param name="sub_addr">address (within the given segment) to deallocate</param>
        public virtual void FreeAtAddress(SegManager segMan, Register sub_addr)
        {
        }

        /// <summary>
        /// Finds the canonic address associated with sub_reg.
        /// Used by the garbage collector.
        /// 
        /// For each valid address a, there exists a canonic address c(a) such that c(a) = c(c(a)).
        /// This address "governs" a in the sense that deallocating c(a) will deallocate a.
        /// </summary>
        /// <param name="segMan"></param>
        /// <param name="sub_addr">base address whose canonic address is to be found</param>
        /// <returns></returns>
        public virtual Register FindCanonicAddress(SegManager segMan, Register sub_addr)
        {
            return sub_addr;
        }
    }

    internal class DataStack : SegmentObj
    {
        /// <summary>
        /// Number of stack entries
        /// </summary>
        public int _capacity;
        public Register[] _entries;

        public DataStack()
            : base(SegmentType.STACK)
        {
        }

        public override bool IsValidOffset(ushort offset)
        {
            return offset < _capacity * 2;
        }

        public override SegmentRef Dereference(Register pointer)
        {
            SegmentRef ret = new SegmentRef();
            ret.isRaw = false;  // reg_t based data!
            ret.maxSize = (int)((_capacity - pointer.Offset / 2) * 2);

            if ((pointer.Offset & 1) != 0)
            {
                ret.maxSize -= 1;
                ret.skipByte = true;
            }

            ret.reg = _entries[pointer.Offset / 2];
            return ret;
        }
    }

    internal class Clone : SciObject
    {
    }

    // Free-style memory
    internal class DynMem : SegmentObj
    {
        public int _size;
        public string _description;
        public byte[] _buf;

        public DynMem() : base(SegmentType.DYNMEM)
        {
        }

        public override bool IsValidOffset(ushort offset)
        {
            return offset < _size;
        }

        public override SegmentRef Dereference(Register pointer)
        {
            SegmentRef ret = new SegmentRef();
            ret.isRaw = true;
            ret.maxSize = (int)(_size - pointer.Offset);
            ret.raw = new ByteAccess(_buf, (int)pointer.Offset);
            return ret;
        }
    }

    internal class SegmentObjTable<T> : SegmentObj where T : class
    {
        private const int HEAPENTRY_INVALID = -1;

        /// <summary>
        /// Statistical information
        /// </summary>
        private int entries_used;
        /// <summary>
        /// Beginning of a singly linked list for entries
        /// </summary>
        private int first_free;

        public class Entry
        {
            public T Item;
            public int next_free; /* Only used for free entries */
        }

        public Entry[] _table;

        public SegmentObjTable(SegmentType type)
            : base(type)
        {
            InitTable();
        }

        public override bool IsValidOffset(ushort offset)
        {
            return IsValidEntry(offset);
        }

        public override List<Register> ListAllDeallocatable(ushort segId)
        {
            var tmp = new List<Register>();
            for (int i = 0; i < _table.Length; i++)
            {
                if (IsValidEntry(i))
                    tmp.Add(Register.Make(segId, (ushort)i));
            }
            return tmp;
        }

        public bool IsValidEntry(int idx)
        {
            return idx >= 0 && (uint)idx < _table.Length && _table[idx].next_free == idx;
        }

        public int AllocEntry()
        {
            entries_used++;
            if (first_free != HEAPENTRY_INVALID)
            {
                int oldff = first_free;
                first_free = _table[oldff].next_free;

                _table[oldff].next_free = oldff;
                return oldff;
            }
            else {
                int newIdx = _table.Length;
                Array.Resize(ref _table, _table.Length + 1);
                _table[newIdx] = new Entry();
                _table[newIdx].next_free = newIdx;  // Tag as 'valid'
                return newIdx;
            }
        }

        private void InitTable()
        {
            entries_used = 0;
            first_free = HEAPENTRY_INVALID;
            Array.Clear(_table, 0, _table.Length);
        }
    }

    internal class CloneTable : SegmentObjTable<Clone>
    {
        public CloneTable()
            : base(SegmentType.CLONES)
        {
        }
    }

    internal class Hunk
    {
        public byte[] mem;
        public int size;
        public string type;
    }

    internal class HunkTable : SegmentObjTable<Hunk>
    {
        public HunkTable() : base(SegmentType.HUNK)
        {
        }

        internal void FreeEntryContents(int idx)
        {
            _table[idx].Item.mem = null;
        }
    }

    internal class LocalVariables : SegmentObj
    {
        /// <summary>
        /// Script ID this local variable block belongs to
        /// </summary>
        public int script_id;
        public Register[] _locals;

        public LocalVariables()
            : base(SegmentType.LOCALS)
        {
        }

        public override bool IsValidOffset(ushort offset)
        {
            return offset < _locals.Length * 2;
        }

        public override SegmentRef Dereference(Register pointer)
        {
            SegmentRef ret = new SegmentRef();
            ret.isRaw = false;  // reg_t based data!
            ret.maxSize = (int)((_locals.Length - pointer.Offset / 2) * 2);

            if ((pointer.Offset & 1) != 0)
            {
                ret.maxSize -= 1;
                ret.skipByte = true;
            }

            if (ret.maxSize > 0)
            {
                ret.reg = _locals[pointer.Offset / 2];
            }
            else {
                if ((SciEngine.Instance.EngineState.CurrentRoomNumber == 160 ||
                     SciEngine.Instance.EngineState.CurrentRoomNumber == 220)
                    && SciEngine.Instance.GameId == SciGameId.LAURABOW2)
                {
                    // WORKAROUND: Happens in two places during the intro of LB2CD, both
                    // from kMemory(peek):
                    // - room 160: Heap 160 has 83 local variables (0-82), and the game
                    //   asks for variables at indices 83 - 90 too.
                    // - room 220: Heap 220 has 114 local variables (0-113), and the
                    //   game asks for variables at indices 114-120 too.
                }
                else {
                    throw new System.InvalidOperationException($"LocalVariables::dereference: Offset at end or out of bounds {pointer}");
                }
                ret.reg = null;
            }
            return ret;
        }
    }

    internal class SegmentRef
    {
        /// <summary>
        /// true if data is raw, false if it is a reg_t sequence
        /// </summary>
        public bool isRaw;
        public ByteAccess raw;
        /// <summary>
        /// number of available bytes
        /// </summary>
        public int maxSize;
        public Register reg;
        // FIXME: Perhaps a generic 'offset' is more appropriate here
        /// <summary>
        /// true if referencing the 2nd data byte of *reg, false otherwise
        /// </summary>
        public bool skipByte;

        public SegmentRef()
        {
            isRaw = true;
            raw = null;
            maxSize = 0;
        }

        public bool IsValid
        {
            get { return (isRaw ? raw != null : !reg.IsNull); }
        }
    }
}
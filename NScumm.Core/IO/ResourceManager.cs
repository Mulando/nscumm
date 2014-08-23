//
//  ResourceManager.cs
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
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace NScumm.Core.IO
{
	public class Script
	{
		public int Id {
			get;
			private set;
		}

		public byte[] Data {
			get;
			private set;
		}

		public Script (int id, byte[] data)
		{
			Id = id;
			Data = data;
		}
	}

	public abstract class ResourceManager
	{
		protected readonly ResourceIndex Index;

		public byte[] ObjectOwnerTable { get { return Index.ObjectOwnerTable; } }

		public byte[] ObjectStateTable { get { return Index.ObjectStateTable; } }

		public uint[] ClassData { get { return Index.ClassData; } }

		public string Directory { get; private set; }

		public IEnumerable<Room> Rooms {
			get {
				var roomIndices = (from res in Enumerable.Range (1, Index.RoomResources.Count - 1)
				                   where res != 0
				                   select (byte)res).Distinct ();
				Room room = null;
				foreach (var i in roomIndices) {
					try {
						room = GetRoom (i);
					} catch (Exception) {
					}
					if (room != null) {
						yield return room;
					}
				}
			}
		}

		public IEnumerable<Script> Scripts {
			get {
				for (byte i = 0; i < Index.ScriptResources.Count; i++) {
					if (Index.ScriptResources [i].RoomNum != 0) {
						byte[] script = null;
						try {
							script = GetScript (i);
						} catch (NotSupportedException) {
							// TODO: mmmh suspicious script error
						}
						if (script != null) {
							yield return new Script (i, script);
						}
					}
				}
			}
		}

		public IEnumerable<byte[]> Sounds {
			get {
				for (byte i = 0; i < Index.SoundResources.Count; i++) {
					if (Index.SoundResources [i].RoomNum != 0) {
						yield return GetSound (i);
					}
				}
			}
		}

		protected ResourceManager (string path)
		{
			Index = ResourceIndex.Load (path);
			Directory = Path.GetDirectoryName (path);
		}

		public static ResourceManager Load (string path, int version)
		{
			switch (version) {
			case 3:
				return new ResourceManager3 (path); 
			case 4:
				return new ResourceManager4 (path); 
			default:
				throw new NotSupportedException (string.Format ("ResourceManager {0} is not supported", version)); 
			}
		}

		static long GetRoomOffset (ResourceFile disk, byte roomNum)
		{
			var rOffsets = disk.ReadRoomOffsets ();
			var roomOffset = rOffsets.ContainsKey (roomNum) ? rOffsets [roomNum] : 0;
			return roomOffset;
		}

		public Room GetRoom (byte roomNum)
		{
			Room room = null;
			var disk = OpenRoom (roomNum);
			if (disk != null) {
				var roomOffset = GetRoomOffset (disk, roomNum);
				room = disk.ReadRoom (roomOffset);
				room.Name = Index.RoomNames != null ? Index.RoomNames [roomNum] : null;
			}

			return room;
		}

		public XorReader GetCostumeReader (byte scriptNum)
		{
			XorReader reader = null;
			var res = Index.CostumeResources [scriptNum];
			var disk = OpenRoom (res.RoomNum);
			if (disk != null) {
				var roomOffset = GetRoomOffset (disk, res.RoomNum);
				reader = disk.ReadCostume (roomOffset + res.Offset);
			}
			return reader;
		}

		public byte[] GetCharsetData (byte id)
		{
			var charset = ReadCharset (id);
			return charset;
		}

		public byte[] GetScript (byte scriptNum)
		{
			byte[] data = null;
			var resource = Index.ScriptResources [scriptNum];
			var disk = OpenRoom (resource.RoomNum);
			if (disk != null) {
				var roomOffset = GetRoomOffset (disk, resource.RoomNum);
				data = disk.ReadScript (roomOffset + resource.Offset);
			}
			return data;
		}

		public byte[] GetSound (int sound)
		{
			byte[] data = null;
			var resource = Index.SoundResources [sound];
			var disk = OpenRoom (resource.RoomNum);
			if (disk != null) {
				var roomOffset = GetRoomOffset (disk, resource.RoomNum);
				data = disk.ReadSound (roomOffset + resource.Offset);
			}
			return data;
		}

		protected abstract ResourceFile OpenRoom (byte roomIndex);

		protected abstract byte[] ReadCharset (byte id);
	}
}

﻿//
//  ImageData.cs
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

using System.Collections.Generic;
using System;

namespace NScumm.Core.Graphics
{
    public class ImageData: ICloneable
    {
        public List<ZPlane>  ZPlanes { get; private set; }

        public byte[] Data { get; set; }

        public bool IsBomp
        {
            get;
            set;
        }

        public ImageData()
        {
            ZPlanes = new List<ZPlane>();
            Data = new byte[0];
        }

        #region ICloneable implementation

        object ICloneable.Clone()
        {
            return Clone();
        }

        public ImageData Clone()
        {
            var data = new ImageData{ IsBomp = IsBomp };
            data.Data = new byte[Data.Length];
            Array.Copy(Data, data.Data, Data.Length);
            foreach (var zplane in ZPlanes)
            {
                data.ZPlanes.Add(zplane.Clone());
            }
            return data;
        }

        #endregion
    }
}


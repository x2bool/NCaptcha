/**
 * BmpFont.cs
 *
 * Author:
 *     Sergey Khabibullin <x2bool@gmail.com>
 *
 * Copyright (c) 2012 Sergey Khabibullin
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NCaptcha
{
	/// <summary>
	/// Bitmap font. Alphabet, bitmap and scale calculation logic.
	/// </summary>
	class BmpFont
	{
		/// <summary>
		/// Alphabet that used in the font
		/// </summary>
		public readonly char[] Alphabet;
		/// <summary>
		/// Contains a raster symbol bitmaps
		/// </summary>
		public readonly Bitmap Bitmap;
		/// <summary>
		/// Font bitmap scale. Represent a start and end coordinates for each symbol in the font
		/// </summary>
		public readonly Dictionary<char, int[]> Scale = new Dictionary<char, int[]> ();
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.BmpFont"/> class.
		/// </summary>
		/// <param name='bitmap'>
		/// Font bitmap. <see cref="NCaptcha.BmpFont.Bitmap"/>
		/// </param>
		/// <param name='alphabet'>
		/// Font alphabet. <see cref="NCaptcha.BmpFont.Alphabet"/>
		/// </param>
		public BmpFont(Bitmap bitmap, char[] alphabet)
		{
			Bitmap = bitmap;
			Alphabet = alphabet;
			
			//
			// parse the bitmap
			//
			
			// add start position of the first symbol
			Scale.Add(Alphabet[0], new int[] { 0, 0 });
	
            for (int x = 1; x < Bitmap.Width; x++)
            {
				// if red channel is equal to 255
                if (Bitmap.GetPixel(x, 0).R == 255)
                {
					// end position
                    Scale[Alphabet[Scale.Count - 1]][1] = x - 1;
					// start position
                    Scale.Add(Alphabet[Scale.Count], new int[] { x + 1, 0 });
                }
            }
			
			// add end position of the last symbol
            Scale[Alphabet[Scale.Count - 1]][1] = Bitmap.Width - 1;
		}
	}
}


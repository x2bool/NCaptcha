/**
 * DllBmpFontPrinter.cs
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
using System.Drawing;
using System.Reflection;

namespace NCaptcha
{
	/// <summary>
	/// Dll bmp font printer.
	/// </summary>
	class DllBmpFontPrinter : BmpFontPrinter
	{
		// alphabet of the assembly fonts
		private char[] alphabet = {
			/*1*/ '2', '3', '4', /*5*/ '6', '7', '8', '9', /*0*/
            'q', 'w', 'e', /*r*/ /*t*/ 'y', 'u', /*i*/ /*o*/ 'p',
            'a', /*s*/ 'd', /*f*/ 'g', 'h', /*j*/ 'k', /*l*/
            'z', 'x', /*c*/ 'v', 'b', 'n', 'm'
		};
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.DllBmpFontPrinter"/> class.
		/// </summary>
		public DllBmpFontPrinter ()
		{
			// retrieve a font names from the assembly
			Assembly dll = Assembly.GetExecutingAssembly();
			string[] fonts = dll.GetManifestResourceNames();
			
			// new font from the random bitmap
			font = new BmpFont(
				new Bitmap (dll.GetManifestResourceStream(fonts[random.Next(0, fonts.Length)])),
				alphabet
			);
		}
	}
}


/**
 * BmpFontPrinter.cs
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

namespace NCaptcha
{
	/// <summary>
	/// Bitmap font printer. Key printing encapsulation for <see cref="NCaptcha.BmpFont"/>
	/// </summary>
	abstract class BmpFontPrinter : IPrinter
	{
		// font bitmap and alphabet container
		protected BmpFont font;
		
		protected Config config = Captcha.Config;
		protected Random random = Captcha.Random;
		
		char[] IPrinter.Alphabet
		{
			// alphabet of currently used font
			get { return font.Alphabet; }
		}
		
		// symbol overlay intensity
		private const int overlayIntensity = 2;
		
		void IPrinter.Print(Bitmap image, string key)
		{
			// canvas coordinates
			int _x, _y;
			// temp image coordinates
			int x, y;
			
			// fill the canvas with a background color
			for (_x = 0; _x < image.Width; _x++)
			{
				for (_y = 0; _y < image.Height; _y++)
				{
					image.SetPixel(_x, _y, config.Background);
				}
			}
			
			// width of temp bitmap; equal to sum of all symbols width
			int width = 0;
			foreach (char symbol in key)
			{
				width += font.Scale[symbol][1] - font.Scale[symbol][0] + 1;
			}
			
			// height of temp bitmap
			int height = (int) (font.Bitmap.Height * 1.3);
			
			// create the temp bitmap
			Bitmap temp = new Bitmap (width, height);
			
			//
			// draw each symbol on the bitmap
			//
			
			x = 0; y = 0;
			
			foreach (char symbol in key)
			{
				// random y margin
				y = random.Next(0, temp.Height - font.Bitmap.Height);
				
				if (x > 1)
				{
					// update x
					x -= overlayIntensity;
				}
				
				//
				// copy current symbol
				//
				
				// from start to end position of current symbol
				for (int font_x = font.Scale[symbol][0]; font_x <= font.Scale[symbol][1]; font_x++)
				{
					// from top to bottom of the font bitmap
					for (int font_y = 2; font_y < font.Bitmap.Height; font_y++)
					{
						// red channel to alpha
						int alpha = 255 - font.Bitmap.GetPixel(font_x, font_y).R + temp.GetPixel(x, y + font_y - 2).A;
						alpha = (alpha > 255) ? 255 : alpha;
						// foreground color with new alpha channel
						temp.SetPixel(x, y + font_y - 2, Color.FromArgb(alpha, config.Foreground));
					}
					
					x++;
				}
			}
			
			//
			// cut (if needed) and copy temp image to center of the canvas
			//
			
			// canvas start positions
			int start_x, start_y;
			// canvas end postions
			int end_x, end_y;
			
			if (x >= image.Width) // if result image width greater than canvas width
			{
				//
				// cut temp image
				//
				
				start_x = 0;
				end_x = image.Width - 1;
				x = (x - end_x) / 2;
			}
			else
			{
				//
				// copy to canvas center
				//
				
				start_x = (image.Width - 1 - x) / 2;
				end_x = start_x + x;
				x = 0;
			}
			
			if (temp.Height > image.Height) // if temp image height greater than canvas height
			{
				//
				// cut temp image
				//
				
				start_y = 0;
				end_y = image.Height - 1;
				y = (temp.Height - image.Height) / 2;
			}
			else
			{
				//
				// copy to canvas center
				//
				
				start_y = (image.Height - temp.Height) / 2;
				end_y = start_y + temp.Height - 1;
				y = 0;
			}
			
			// save y
			int Y = y;
			
			for (_x = start_x; _x <= end_x; _x++, x++)
			{
				y = Y;
				
				for (_y = start_y; _y <= end_y; _y++, y++)
				{
					Color tempPixel = temp.GetPixel(x, y);
					
					if (tempPixel.A == 0)
					{
						continue;
					}
					
					Color canvasPixel = image.GetPixel(_x, _y);
					
					double transparency = tempPixel.A / 255.0;
					
					// alpha compositing
					int red = (int) (tempPixel.R * transparency + canvasPixel.R * (1.0 - transparency));
					int green = (int) (tempPixel.G * transparency + canvasPixel.G * (1.0 - transparency));
					int blue = (int) (tempPixel.B * transparency + canvasPixel.B * (1.0 - transparency));
					
					// replace pixel color
					image.SetPixel(_x, _y, Color.FromArgb(red, green, blue));
				}
			}
			
		}
	}
}


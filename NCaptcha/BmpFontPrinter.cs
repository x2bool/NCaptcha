/*
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
		
		void IPrinter.Print(Bitmap image, string key)
		{
			// fill the canvas with background color
			int _x, _y; // canvas coordinates
			for (_x = 0; _x < image.Width; _x++)
			{
				for (_y = 0; _y < image.Height; _y++)
				{
					image.SetPixel(_x, _y, config.Background);
				}
			}
			
			//  width of temp bitmap
			int width = 0;
			// height of temp bitmap
			int height = (int) (font.Bitmap.Height * 1.3);
			
			// calculate temp bitmap width; equal to sum of all symbols width
			foreach (char symbol in key)
			{
				width += font.Scale[symbol][1] - font.Scale[symbol][0] + 1;
			}
			
			// create the temp bitmap
			Bitmap temp = new Bitmap (width, height);
			
			// draw each symbol on the bitmap
			int x = 0, y;
			foreach (char symbol in key)
			{
				// random y margin
				y = random.Next(0, height - font.Bitmap.Height);
				
				if (x > 0)
				{
					// update x
					x -= 2;
				}
				
				// copy current symbol
				for (int font_x = font.Scale[symbol][0]; font_x <= font.Scale[symbol][1]; font_x++)
				{
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
			
			// cut (if needed) and copy temp image to center of canvas 
			int end_x, end_y;
			
			x++;
			
			if (x > image.Width)
			{
				// temp start postion
				x = (x - image.Width) / 2;
				// canvas start position
				_x = 0;
				// canvas end position
				end_x = image.Width;
			}
			else
			{
				// canvas start position
				_x = (image.Width - x) / 2;
				// canvas end position
				end_x = _x + x - 1;
				// image start position
				x = 0;
			}
			
			if (temp.Height > image.Height)
			{
				// temp start position
				y = (temp.Height - image.Height) / 2;
				// canvas start position
				_y = 0;
				// canvas end position
				end_y = image.Height;
			}
			else
			{
				// canvas start position
				_y = (image.Height - temp.Height) / 2;
				// end position of canvas
				end_y = _y + temp.Height - 1;
				// start position of image
				y = 0;
			}
			
			// save start y positions
			int Y = y, _Y = _y;
			
			while (_x < end_x)
			{
				// reset y coordinates
				y = Y; _y = _Y;
				while (_y < end_y)
				{
					Color tempPixel = temp.GetPixel(x, y);
					
					if (tempPixel.A == 0)
					{
						goto next_pixel;
					}
					
					Color canvasPixel = image.GetPixel(_x, _y);
					
					double transparency = tempPixel.A / 255.0;
					
					// alpha compositing
					int red = (int) (tempPixel.R * transparency + canvasPixel.R * (1.0 - transparency));
					int green = (int) (tempPixel.G * transparency + canvasPixel.G * (1.0 - transparency));
					int blue = (int) (tempPixel.B * transparency + canvasPixel.B * (1.0 - transparency));
					
					// replace pixel color
					image.SetPixel(_x, _y, Color.FromArgb(red, green, blue));
					
				next_pixel:
					y++; _y++;
				}
				
				x++; _x++;
			}
			
		}
	}
}


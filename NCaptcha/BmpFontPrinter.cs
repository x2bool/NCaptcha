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
			// alphabet of the currently using font
			get { return font.Alphabet; }
		}
		
		// the symbol overlay intensity
		private const int overlayIntensity = 0;
		// the overlaying treshold
		private const int overlayTreshold = 160; // from 1 to 255
		
		void IPrinter.Print(Bitmap image, string key)
		{
			// canvas coordinates
			int _x, _y;
			// temp image coordinates
			int x, y;
			// font bitmap coordinates
			int fx, fy;
			
			// fill the canvas with the background color
			for (_x = 0; _x < image.Width; _x++)
			{
				for (_y = 0; _y < image.Height; _y++)
				{
					image.SetPixel(_x, _y, config.Background);
				}
			}
			
			// the width of temp bitmap; equal to sum of all symbols width
			int width = 0;
			foreach (char symbol in key)
			{
				width += font.Scale[symbol][1] - font.Scale[symbol][0] + 1;
			}
			
			// the height of the temp bitmap
			int height = (int) (font.Bitmap.Height * 1.2);
			
			// create the temp bitmap
			Bitmap temp = new Bitmap (width, height);
			
			//
			// draw each symbol on the bitmap
			//
			
			// array with a last x position of significant (dark) pixel on each y
			int[] border = new int[height];
			for (int i = 0; i < height; i++)
			{
				border[i] = 0;
			}
			
			x = 0; y = 0;
			
			foreach (char symbol in key)
			{
				// random y margin
				y = random.Next(0, temp.Height - font.Bitmap.Height);
				
				if (config.Overlay == true && x > 0)
				{
					//
					// update x
					//
					
					// maximum possible value
					int shift = int.MaxValue;
					
					// start position of current symbol
					fx = font.Scale[symbol][0];
					// from current x posistion to value that is equal to (x + symbol width)
					for (int xx = x; fx <= font.Scale[symbol][1]; xx++, fx++)
					{
						// don't use the pixels of the scale
						fy = 2;
						// from current y to (y + font height)
						for (int yy = y; fy < font.Bitmap.Height; yy++, fy++)
						{
							// if font pixel has enough value of red channel
							if (font.Bitmap.GetPixel(fx, fy).R <= overlayTreshold)
							{
								// get distance between pixels
								int dist = xx - border[yy];
								// if pixel is the nearest 
								if (dist < shift)
								{
									// "shift" will be equal to this distance
									shift = dist;
								}
							}
						}
						
						// if distance can't be smaller
						if (xx - x >= shift)
						{
							break;
						}
					}
					
					// new x
					x -= shift + overlayIntensity;
				}
				
				//
				// copy current symbol
				//
				
				// from the start to the end position of current symbol
				for (fx = font.Scale[symbol][0]; fx <= font.Scale[symbol][1]; fx++)
				{
					// from the top to the bottom of the font bitmap
					for (fy = 2; fy < font.Bitmap.Height; fy++)
					{
						// convert red channel to alpha
						int alpha = 255 - font.Bitmap.GetPixel(fx, fy).R + temp.GetPixel(x, y + fy - 2).A;
						alpha = (alpha > 255) ? 255 : alpha;
						// foreground color with new alpha channel
						temp.SetPixel(x, y + fy - 2, Color.FromArgb(alpha, config.Foreground));
						
						// add last pixel to "border" array if the alpha greater than overlay treshold
						if (alpha >= 255 - overlayTreshold)
						{
							border[y + fy - 2] = x;
						}
					}
					
					x++;
				}
			}
			
			//
			// cut (if need) and copy the temp image to center of the canvas
			//
			
			// canvas start positions
			int start_x, start_y;
			// canvas end postions
			int end_x, end_y;
			
			if (x >= image.Width) // if result image width greater than the canvas width
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
				end_x = start_x + x - 1;
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
			
			//
			// process image
			//
			
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


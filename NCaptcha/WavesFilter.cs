/**
 * WavesFilter.cs
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
	/// Waves filter. Image distorter
	/// </summary>
	class WavesFilter : IFilter
	{
		/// <summary>
		/// Wave encapsulation
		/// </summary>
		private struct Wave
		{
			public double Period;
			public double Phase;
			public double Amplitude;
		}
		
		// x waves
		private Wave bigWaveX;
		private Wave smallWaveX;
		
		// y waves
		private Wave bigWaveY;
		private Wave smallWaveY;
		
		private Random random = Captcha.Random;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.WavesFilter"/> class.
		/// </summary>
		public WavesFilter ()
		{
			bigWaveX = new Wave {
				Period = 0.15,
				Phase = random.Next(0, 628) / 100.0, // from 0 to 2*Math.PI
				Amplitude = 2
			};
			smallWaveX = new Wave {
				Period = 0,
				Phase = 0,
				Amplitude = 0 // no small x wave
			};
			
			bigWaveY = new Wave {
				Period = 0.040,
				Phase = random.Next(0, 628) / 100.0, // from 0 to 2*Math.PI
				Amplitude = random.Next(6, 8) // 6 or 7
			};
			smallWaveY = new Wave {
				Period = 0.1,
				Phase = random.Next(0, 628) / 100.0, // from 0 to 2*Math.Pi
				Amplitude = random.Next(2, 4) // 2 or 3
			};
		}
		
		void IFilter.Process(Bitmap image)
		{
			// copy image
			Bitmap copy = (Bitmap) image.Clone();
			
			// each image pixel
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					// apply sin function to x; composition of the small and big waves
					double _x = x + (
						bigWaveX.Amplitude * Math.Sin(bigWaveX.Period * y + bigWaveX.Phase)
						+ smallWaveX.Amplitude * Math.Sin(smallWaveX.Period * y + smallWaveX.Phase)
					);
					
					// apply sin function to y; composition of the small and big waves
					double _y = y + (
						bigWaveY.Amplitude * Math.Sin(bigWaveY.Period * x + bigWaveY.Phase)
						+ smallWaveY.Amplitude * Math.Sin(smallWaveY.Period * x + smallWaveY.Phase)
					);
					
					// x and y on image copy
					int old_x = (int) _x;
					int old_y = (int) _y;
					
					double fract_x = _x - old_x;
					double fract_y = _y - old_y;
					
					// check if old_x and old_y out of image sizes
					old_x = (old_x > 0) ? (old_x < image.Width) ? old_x : image.Width - 1 : 0;
					old_y = (old_y > 0) ? (old_y < image.Height) ? old_y : image.Height - 1 : 0;
					
					// next pixels on x and y
					int old_x1 = old_x + 1;
					int old_y1 = old_y + 1;
					
					// normalize 
					old_x1 = (old_x1 < image.Width) ? old_x1 : old_x;
					old_y1 = (old_y1 < image.Height) ? old_y1 : old_y;
					
					// current pixel; x, y
					Color color_00 = copy.GetPixel(old_x, old_y);
					// x, y + 1
					Color color_01 = copy.GetPixel(old_x, old_y1);
					// x + 1, y
					Color color_10 = copy.GetPixel(old_x1, old_y);
					// x + 1, y + 1
					Color color_11 = copy.GetPixel(old_x1, old_y1);
					
					//
					// rgb linear interpolation
					//
					
					int r = (int) (
						color_00.R * (1 - fract_x) * (1 - fract_y)
						+ color_01.R * (1 - fract_x) * fract_y
						+ color_10.R * fract_x * (1 - fract_y)
						+ color_11.R * fract_x * fract_y
					);
					
					int g = (int) (
						color_00.G * (1 - fract_x) * (1 - fract_y)
						+ color_01.G * (1 - fract_x) * fract_y
						+ color_10.G * fract_x * (1 - fract_y)
						+ color_11.G * fract_x * fract_y
					);
					
					int b = (int) (
						color_00.B * (1 - fract_x) * (1 - fract_y)
						+ color_01.B * (1 - fract_x) * fract_y
						+ color_10.B * fract_x * (1 - fract_y)
						+ color_11.B * fract_x * fract_y
					);
					
					// rgb normalization (between 0 and 255)
					r = (r > 0) ? (r < 255) ? r : 255 : 0;
					g = (g > 0) ? (g < 255) ? g : 255 : 0;
					b = (b > 0) ? (b < 255) ? b : 255 : 0;
					
					// set new pixel
					image.SetPixel(x, y, Color.FromArgb(r, g, b));
				}
			}
		}
	}
}

 
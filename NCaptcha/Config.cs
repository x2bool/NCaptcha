/*
 * Config.cs
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
	/// Config.
	/// </summary>
	/// <exception cref='NCaptcha.ConfigException'>
	/// Represents errors that occur during application execution.
	/// </exception>
	class Config
	{
		#region default configuration
		/// <summary>
		/// The length of the key.
		/// </summary>
		public readonly int KeyLength = 0;
		
		/// <summary>
		/// The width.
		/// </summary>
		public readonly int Width = 100;
		
		/// <summary>
		/// The height.
		/// </summary>
		public readonly int Height = 50;
		
		/// <summary>
		/// The foreground.
		/// </summary>
		public readonly Color Foreground;
		
		/// <summary>
		/// The background.
		/// </summary>
		public readonly Color Background;
		
		/// <summary>
		/// The waves.
		/// </summary>
		public readonly bool Waves = false;
		
		/// <summary>
		/// The noise.
		/// </summary>
		public readonly bool Noise = false;
		#endregion
		
		private Random random = Captcha.Random;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.Config"/> class.
		/// </summary>
		/// <param name='config'>
		/// Config.
		/// </param>
		/// <exception cref='NCaptcha.ConfigException'>
		/// Represents errors that occur during application execution.
		/// </exception>
		private Config(object config)
		{
			if (config != null)
			{
				PropertyInfo[] props = config.GetType().GetProperties();
				
				foreach (PropertyInfo prop in props)
				{
					object val = prop.GetValue(config, null);
					string type = prop.PropertyType.FullName;
					
					switch (prop.Name)
					{
					/* key length*/
					case "keylength":
						switch (type)
						{
							// only integers
						case "System.Int16":
						case "System.Int32":
							KeyLength = (int) val;
							break;
						default:
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						
						if (KeyLength < 1)
						{
							throw new ConfigException(prop.Name, "Illegal value range");
						}
						
						break;
					/* image width */
					case "width":
						switch (type)
						{
							// only integers
						case "System.Int16":
						case "System.Int32":
							Width = (int) val;
							break;
						default:
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						
						if (Width < 1)
						{
							throw new ConfigException(prop.Name, "Illegal value range");
						}
						break;
					/* image height*/
					case "height":
						switch (type)
						{
							// only integers
						case "System.Int16":
						case "System.Int32":
							Height = (int) val;
							break;
						default:
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						if (Height < 1)
						{
							throw new ConfigException(prop.Name, "Illegal value range");
						}
						break;
						
					/* foreground (text) color */
					case "foreground":
						switch (type)
						{
							// color
						case "System.Drawing.Color":
							Foreground = (Color) val;
							break;
							// html color string #RRGGBB
						case "System.String":
							try
							{
								Foreground = ColorTranslator.FromHtml((string) val);
							}
							catch
							{
								throw new ConfigException(prop.Name, "Incorrect html color");
							}
							break;
						default:
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						
						// alpha channel is illegal
						if (Foreground.A != 255)
						{
							throw new ConfigException(prop.Name, "Illegal alpha channel value");
						}
						break;
					/* background color */
					case "background":
						switch (type)
						{
							// color
						case "System.Drawing.Color":
							Background = (Color) val;
							break;
							// html color string #RRGGBB
						case "System.String":
							try
							{
								Background = ColorTranslator.FromHtml((string) val);
							}
							catch
							{
								throw new ConfigException(prop.Name, "Incorrect html color");
							}
							break;
						default:
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						
						// alpha channel is illegal
						if (Background.A != 255)
						{
							throw new ConfigException(prop.Name, "Illegal alpha channel value");
						}
						break;
					/* waves filter */
					case "waves":
						// only boolean
						if (type == "System.Boolean")
						{
							Waves = (bool) val;
						}
						else
						{
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						break;
					/* noise filter */
					case "noise":
						// only boolean
						if (type == "System.Boolean")
						{
							Noise = (bool) val;
						}
						else
						{
							throw new ConfigException(prop.Name, "Incorrect value type");
						}
						break;
					/* unknown parameter */
					default:
						throw new ConfigException("Unknown configuration parameter '"+prop.Name+"'");
					}
					
				}
			}
			
			if (KeyLength == 0)
			{
				// random. 5 or 6
				KeyLength = random.Next(5, 7);
			}
			
			if (!Foreground.IsEmpty)
			{
				// if we have only foreground
				if (Background.IsEmpty)
				{
					// background by foreground
					Background = GetDerivedColor(Foreground);
				}
				/*
				else both colors specifed; do nothing
				*/
			}
			// only background is specifed
			else if (!Background.IsEmpty)
			{
				// foreground by background
				Foreground = GetDerivedColor(Background);
			}
			// no one color
			else
			{
				// random foreground
				Foreground = Color.FromArgb(
					random.Next(0x20, 0x40),
					random.Next(0x20, 0x40),
					random.Next(0x20, 0x40)
				);
				// background by foreground
				Background = GetDerivedColor(Foreground);
			}
			
		}
		
		private Color GetDerivedColor(Color baseColor)
		{
			int r = random.Next(0x80, 0xA0),
				g = random.Next(0x80, 0xA0),
				b = random.Next(0x80, 0xA0);
			
			if (baseColor.GetBrightness() > 0.5)
			{
				// reverse
				r = -r; g = -g; b = -b;
			}
			
			// get derived rgb
			r += baseColor.R;
			g += baseColor.G;
			b += baseColor.B;
			
			// rgb normalization
			return Color.FromArgb(
				r > 255 ? 255 : r > 0 ? r : 0,
				g > 255 ? 255 : g > 0 ? g : 0,
				b > 255 ? 255 : b > 0 ? b : 0
			);
		}
		
		public static Config Init(object config)
		{
			return new Config(config);
		}
	}
}


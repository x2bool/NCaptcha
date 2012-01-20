/**
 * Captcha.cs
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
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("NCaptcha")]
[assembly: AssemblyDescription(".NET/Mono CAPTCHA")]
[assembly: AssemblyCopyright("© 2012 Sergey Khabibullin")]
[assembly: AssemblyVersion("0.9.0")]

namespace NCaptcha
{
	/// <summary>
	/// Captcha. Automatic test to tell computers and humans apart.
	/// </summary>
	public class Captcha
	{
		/// <summary>
		/// Captcha key
		/// </summary>
		public readonly string Key;
		/// <summary>
		/// Captcha image.
		/// </summary>
		public readonly Bitmap Image;
		
		// configuration provider for other classes
		internal static Config Config { get; private set; }
		// random generator provideer for other assembly classes
		internal static Random Random { get; private set; }
		
		// filter delegate
		private delegate void Filter(Bitmap image);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.Captcha"/> class.
		/// </summary>
		/// <param name='config'>
		/// Config.
		/// </param>
		public Captcha (object config = null)
		{
			Random = new Random ();
			
			// parse configuration
			Config = Config.Init(config);
			
			// create the new canvas
			Image = new Bitmap(Config.Width, Config.Height);
			
			IPrinter printer = new DllBmpFontPrinter ();
			
			IKeygen keygen = new SmartKeygen ();
			
			// key generation
			Key = keygen.Generate(printer.Alphabet);
			// key drawing
			printer.Print(Image, Key);
			
			// empty filter
			Filter filters = (image) => { /* do nothing */ };
			
			if (Config.Waves == true)
			{
				// add the waves filter
				IFilter waves = new WavesFilter ();
				filters += waves.Process;
			}
			
			if (Config.Noise == true)
			{
				// add the noise filter
				IFilter noise = new NoiseFilter ();
				filters += noise.Process;
			}
			
			// apply filters
			filters(Image);
		}
	}
}


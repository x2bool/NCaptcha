/*
 * SmartKeygen.cs
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

namespace NCaptcha
{
	/// <summary>
	/// Smart keygen.
	/// </summary>
	class SmartKeygen : IKeygen
	{
		// preventing from bad readability
		private char[][] deniedPairs = {
			new char[] {'n', 'n'}, new char[] {'n', 'm'}, new char[] {'m', 'n'}, new char[] {'m', 'm'},
			new char[] {'v', 'v'}, new char[] {'v', 'w'}, new char[] {'w', 'v'}, new char[] {'w', 'w'},
			new char[] {'q', 'p'}, new char[] {'q', 'b'}, new char[] {'q', 'h'}
		};
		
		private Config config = Captcha.Config;
		private Random random = Captcha.Random;
		
		string IKeygen.Generate(char[] alphabet)
		{
			char[] key = new char[config.KeyLength];
			
			for (int i = 0; i < key.Length;)
			{
				// random symbol from alphabet
				char symbol = alphabet[random.Next(0, alphabet.Length)];
				
				if (i > 0)
				{
					// regenerate if previus and current symbols in the deniedPairs array
					foreach (char[] pair in deniedPairs)
					{
						if (pair[0] == key[i - 1] && pair[1] == symbol)
						{
							// try again
							goto regenerate;
						}
					}
				}
				
				// add the symbol
				key[i] = symbol;
				
				// next step
				i++;
				
			regenerate:
					continue;
			}
			
			return new string(key);
		}
	}
}


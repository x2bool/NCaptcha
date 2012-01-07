/*
 * ConfigException.cs
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
	/// Config exception.
	/// </summary>
	public class ConfigException : Exception
	{
		/// <summary>
		/// The parameter.
		/// </summary>
		public readonly string parameter;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.ConfigException"/> class.
		/// </summary>
		/// <param name='message'>
		/// Message.
		/// </param>
		public ConfigException (string message) : base (message) { }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="NCaptcha.ConfigException"/> class.
		/// </summary>
		/// <param name='parameter'>
		/// Parameter.
		/// </param>
		/// <param name='message'>
		/// Message.
		/// </param>
		public ConfigException (string parameter, string message) : base (message)
		{
			this.parameter = parameter;
		}
	}
}


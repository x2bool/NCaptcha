//
// NumberGenerator.cs
//
// Author:
//       Sergey Khabibullin <sergey@khabibullin.com>
//
// Copyright (c) 2014 Sergey Khabibullin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace NCaptcha
{
    public sealed class NumberGenerator : INumberGenerator
    {
        Random random;

        NumberGenerator()
        {
            this.random = new Random();
        }

        public static NumberGenerator Instance
        {
            get
            {
                return Singleton.instance;
            }
        }

        public int NextInt(int min, int max)
        {
            lock (random)
            {
                return random.Next(min, max);
            }
        }

        class Singleton
        {
            // do not mark type as beforefieldinit
            static Singleton() { }
            internal static readonly NumberGenerator instance = new NumberGenerator();
        }
    }

    public interface INumberGenerator
    {
        int NextInt(int min, int max);
    }
}


//
// FilterFactory.cs
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
using NCaptcha.Configuration;

namespace NCaptcha.Utils.Factories
{
    public static class FilterFactory
    {
        public static IFilter[] Create(Config config)
        {
            if (config.WavesFilterEnabled)
            {
                var random = NumberGenerator.Instance;

                var xWave = new WavesFilter.Wave(0.15f, 2, randomPhase(random));
                var bigYWave = new WavesFilter.Wave(0.040f, random.NextInt(6, 8), randomPhase(random));
                var smallYWave = new WavesFilter.Wave(0.1f, random.NextInt(2, 4), randomPhase(random));

                return new IFilter[]
                {
                    new WavesFilter(
                        new WavesFilter.Wave[] { xWave },
                        new WavesFilter.Wave[] { bigYWave, smallYWave })
                };
            }
            else
            {
                return new IFilter[0];
            }
        }

        static float randomPhase(INumberGenerator random)
        {
            return random.NextInt(0, 628) / 100f; // 0..2PI
        }
    }
}


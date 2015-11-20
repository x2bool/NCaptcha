//
// Keygen.cs
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

namespace NCaptcha.Utils
{
    public sealed class Keygen : IKeygen
    {
        const int MAX_ATTEMPS = 5;

        public int KeyLength { get; set; }
        public char[] Alphabet { get; set; }

        public struct Pair
        {
            public readonly char First;
            public readonly char Second;

            public Pair(char first, char second)
            {
                First = first;
                Second = second;
            }
        }

        // prevent bad readability
        Pair[] deniedPairs =
        {
            new Pair('n', 'n'), new Pair('n', 'm'), new Pair('m', 'n'), new Pair('m', 'm'),
            new Pair('v', 'v'), new Pair('v', 'w'), new Pair('w', 'v'), new Pair('w', 'w'),
            new Pair('q', 'p'), new Pair('q', 'b'), new Pair('q', 'h')
        };

        readonly INumberGenerator random;

        public Keygen(INumberGenerator random)
        {
            this.random = random;
        }

        public string GenerateKey()
        {
            char[] key = new char[KeyLength];

            int attempt = 1;

            for (int i = 0; i < key.Length; attempt++)
            {
                // random symbol from the alphabet
                char symbol = Alphabet[random.NextInt(0, Alphabet.Length)];

                if (i > 0)
                {
                    // regenerate if the previus and current symbols in the "deniedPairs" array
                    foreach (var pair in deniedPairs)
                    {
                        if (pair.First == key[i - 1] && pair.Second == symbol)
                        {
                            // do not try to regenerate if too many attempts are made
                            if (attempt <= MAX_ATTEMPS)
                            {
                                // try again
                                goto regenerate; // http://imgs.xkcd.com/comics/goto.png
                            }
                        }
                    }

                    attempt = 0;
                }

                // add the symbol
                key[i++] = symbol;

                regenerate: continue;
            }

            return new string(key);
        }
    }
}


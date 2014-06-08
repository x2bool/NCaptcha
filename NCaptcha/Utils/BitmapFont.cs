//
// BitmapFont.cs
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
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NCaptcha.Utils
{
    public sealed class BitmapFont
    {
        public struct Bounds
        {
            public readonly int Start;
            public readonly int End;

            public Bounds(int start, int end)
            {
                Start = start;
                End = end;
            }
        }

        public readonly Bitmap Bitmap;
        public readonly int Treshold;

        readonly Dictionary<char, Bounds> bounds;

        public char[] Alphabet
        {
            get
            {
                int i = 0;
                var alphabet = new char[bounds.Count];

                foreach (var item in bounds)
                {
                    alphabet[i++] = item.Key;
                }

                return alphabet;
            }
        }

        public Bounds this[char c]
        {
            get
            {
                try
                {
                    return bounds[c];
                }
                catch (KeyNotFoundException)
                {
                    throw new KeyNotFoundException("The character is not supported by the font");
                }
            }
        }

        public BitmapFont(Bitmap bitmap, Dictionary<char, Bounds> bounds)
        {
            Bitmap = bitmap;
            this.bounds = bounds;
        }

        public static BitmapFont FromStream(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream))
            {
                char[] meta;

                try
                {
                    meta = readMetadata(bufferedStream);
                }
                catch (Exception e)
                {
                    throw new Exception("Can't read meta information from stream", e);
                }

                if (meta == null || meta.Length == 0)
                {
                    throw new Exception("Resource doesn't contain meta information");
                }

                char key = '0';
                int start = 0;

                var bounds = new Dictionary<char, Bounds>();

                // read bounds {key}{start}{end}...{key}{start}{end}
                for (int i = 0, j = 1; i < meta.Length; i++, j++)
                {
                    char c = meta[i];

                    if (j == 3) // every third char is the end position
                    {
                        j = 0;
                        bounds.Add(key, new Bounds(start, (int) c));
                    }
                    else if (j == 2) // the second char is the start position
                    {
                        start = (int) c;
                    }
                    else // the first char is a key
                    {
                        key = c;
                    }
                }

                bufferedStream.Seek(0, SeekOrigin.Begin);
                return new BitmapFont(new Bitmap(bufferedStream), bounds);
            }
        }

        // http://www.libpng.org/pub/png/spec/1.2/PNG-Chunks.html
        static readonly byte[] png = { 137, 80, 78, 71, 13, 10, 26, 10 }; // header
        static readonly byte[] txt = { 105, 84, 88, 116 }; // iTXt
        static readonly byte[] end = { 73, 69, 78, 68 }; // IEND

        // keyword
        static readonly char[] key = "NCaptcha.FontMeta".ToCharArray();

        static bool equal<T>(T[] arr1, T[] arr2)
        {
            if (arr1.Length != arr2.Length)
            {
                return false;
            }

            for (int i = 0; i < arr1.Length; i++)
            {
                if (!arr1[i].Equals(arr2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static int toInt (byte[] bytes)
        {
            uint sum = 0;

            for (int i = 0; i < 4; i++)
            {
                sum |= (uint) bytes[i] << ((3 - i) * 8);
            }

            return (int) sum;
        }

        static char[] readMetadata(Stream stream)
        {
            // read png header
            var header = new byte[8];
            stream.Read(header, 0, png.Length);

            if (!equal<byte>(header, png))
            {
                throw new Exception("Resource isn't a valid png image");
            }

            char[] meta = null;

            var length = new byte[4];
            var type = new byte[4];

            // read png chunks
            while (true)
            {
                stream.Read(length, 0, 4);
                stream.Read(type, 0, 4);

                int l = toInt(length);

                if (equal<byte>(type, txt))
                {
                    var data = new byte[l];

                    // iTXt Chunk
                    stream.Read(data, 0, l);
                    stream.Seek(4, SeekOrigin.Current); // CRC bytes

                    // Keyword:             1-79 bytes (character string)
                    // Null separator:      1 byte
                    // Compression flag:    1 byte
                    // Compression method:  1 byte
                    // Language tag:        0 or more bytes (character string)
                    // Null separator:      1 byte
                    // Translated keyword:  0 or more bytes
                    // Null separator:      1 byte
                    // Text:                0 or more bytes

                    int null1 = Int32.MinValue;
                    int null2 = Int32.MinValue;
                    int null3 = Int32.MinValue;

                    // find all null separator indexes
                    for (int i = 0; i < l; i++)
                    {
                        if (data[i] == 0)
                        {
                            if (null1 == Int32.MinValue)
                            {
                                null1 = i;
                                i += 2; // skip compression bytes
                            }
                            else if (null2 == Int32.MinValue)
                            {
                                null2 = i;
                            }
                            else
                            {
                                null3 = i;
                                break;
                            }
                        }
                    }

                    // if keyword is equal to metadata key
                    if (equal<char>(key, Encoding.GetEncoding("ISO-8859-1").GetChars(data, 0, null1)))
                    {
                        // read text, which starts after the last null separator
                        meta = Encoding.UTF8.GetChars(data, null3 + 1, l - null3 - 1);
                        break;
                    }
                }
                else if (equal<byte>(type, end))
                {
                    // the end of the png file
                    break;
                }
                else
                {
                    // skip (length + CRC) any chunk that is not iTXt or IEND
                    stream.Seek(l + 4, SeekOrigin.Current);
                }
            }

            return meta;
        }
    }
}
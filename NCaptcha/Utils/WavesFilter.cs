//
// WavesFilter.cs
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
using NCaptcha.Configuration;

namespace NCaptcha.Utils
{
    public sealed class WavesFilter : IFilter
    {
        public struct Wave
        {
            public readonly float Period;
            public readonly float Amplitude;
            public readonly float Phase;

            public Wave(float period, float amplitude, float phase)
            {
                Period = period;
                Amplitude = amplitude;
                Phase = phase;
            }
        }

        Wave[] x_waves;
        Wave[] y_waves;

        public WavesFilter(Wave[] x_waves, Wave[] y_waves)
        {
            this.x_waves = x_waves;
            this.y_waves = y_waves;
        }

        public Image Apply(Image image)
        {
            var bitmap = (Bitmap) image;

            // copy image
            Bitmap copy = new Bitmap(bitmap);

            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    float _x = x + sin(y, x_waves);
                    float _y = y + sin(x, y_waves);

                    // x and y coordinates on copy
                    int old_x = (int) _x;
                    int old_y = (int) _y;

                    // fractional part of coordinates
                    float fract_x = _x - old_x;
                    float fract_y = _y - old_y;

                    // make sure old_x and old_y are not out of image
                    old_x = (old_x > 0) ? (old_x < bitmap.Width) ? old_x : bitmap.Width - 1 : 0;
                    old_y = (old_y > 0) ? (old_y < bitmap.Height) ? old_y : bitmap.Height - 1 : 0;

                    // next pixels on x and y
                    int old_x1 = old_x + 1;
                    int old_y1 = old_y + 1;

                    // make sure x and y are staying within the image bounds
                    old_x1 = (old_x1 < bitmap.Width) ? old_x1 : old_x;
                    old_y1 = (old_y1 < bitmap.Height) ? old_y1 : old_y;

                    // current pixel; x, y
                    Color color_00 = copy.GetPixel(old_x, old_y);
                    // x, y + 1
                    Color color_01 = copy.GetPixel(old_x, old_y1);
                    // x + 1, y
                    Color color_10 = copy.GetPixel(old_x1, old_y);
                    // x + 1, y + 1
                    Color color_11 = copy.GetPixel(old_x1, old_y1);

                    int r = interpolate(color_00.R, color_01.R, color_10.R, color_11.R, fract_x, fract_y);
                    int g = interpolate(color_00.G, color_01.G, color_10.G, color_11.G, fract_x, fract_y);
                    int b = interpolate(color_00.B, color_01.B, color_10.B, color_11.B, fract_x, fract_y);

                    bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            return bitmap;
        }

        static float sin(int n, Wave[] waves)
        {
            float sum = 0.0f;
            foreach(var wave in waves)
            {
                sum += wave.Amplitude * (float) Math.Sin(wave.Period * n + wave.Phase);
            }

            return sum;
        }

        static int interpolate(int c_00, int c_01, int c_10, int c_11, float fract_x, float fract_y)
        {
            int c = (int) (
                c_00 * (1 - fract_x) * (1 - fract_y)
                + c_01 * (1 - fract_x) * fract_y
                + c_10 * fract_x * (1 - fract_y)
                + c_11 * fract_x * fract_y
            );

            return (c > 0) ? (c < 255) ? c : 255 : 0;
        }
    }
}
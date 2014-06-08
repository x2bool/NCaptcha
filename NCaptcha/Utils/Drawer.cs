//
// Drawer.cs
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

namespace NCaptcha.Utils
{
    using NCaptcha.Configuration;

    public sealed class Drawer : IDrawer
    {
        public BitmapFont Font { get; set; }
        public Color FontColor { get; set; }
        public bool OverlayEnabled { get; set; }
        public int OverlayPixels { get; set; }

        readonly INumberGenerator random;

        public Drawer(INumberGenerator random)
        {
            this.random = random;
        }

        public Image DrawKey(Image image, string key)
        {
            var bitmap = (Bitmap) image;

            // canvas coordinates
            int image_x = 0, image_y = 0;
            // temp image coordinates
            int temp_x = 0, temp_y = 0;
            // font bitmap coordinates
            int font_x, font_y;

            // the width of temp bitmap is equal to width sum
            int width = 0;
            foreach (char symbol in key)
            {
                var bounds = Font[symbol];
                width += bounds.End - bounds.Start + 1;
            }

            // height of the temp bitmap
            int height = (int) (Font.Bitmap.Height * 1.3);

            // create temp bitmap
            Bitmap temp = new Bitmap (width, height);

            // array with last x position of significant (dark) pixel on each y
            int[] lastPosition = new int[height];

            foreach (char symbol in key)
            {
                // random y margin
                temp_y = random.NextInt(0, temp.Height - Font.Bitmap.Height);

                var bounds = Font[symbol];

                if (OverlayEnabled && temp_x > 0)
                {
                    // maximum possible value
                    int shift = int.MaxValue;

                    // start position of current symbol
                    font_x = bounds.Start;

                    // from current x posistion to value that is equal to (x + symbol width)
                    for (int xx = temp_x; font_x <= bounds.End; xx++, font_x++)
                    {
                        font_y = 0;

                        // from current y to (y + font height)
                        for (int yy = temp_y; font_y < Font.Bitmap.Height; yy++, font_y++)
                        {
                            // if font pixel has enough value of red channel
                            if (Font.Bitmap.GetPixel(font_x, font_y).R <= Font.Treshold)
                            {
                                // get distance between pixels
                                int dist = xx - lastPosition[yy];
                                // if pixel is the nearest
                                if (dist < shift)
                                {
                                    // "shift" will be equal to this distance
                                    shift = dist;
                                }
                            }
                        }

                        // if distance can't be smaller
                        if (xx - temp_x >= shift)
                        {
                            break;
                        }
                    }

                    // new x
                    temp_x -= shift + OverlayPixels;
                }

                // from the start to the end position of current symbol
                for (font_x = bounds.Start; font_x <= bounds.End; font_x++)
                {
                    // from the top to the bottom of the font bitmap
                    for (font_y = 0; font_y < Font.Bitmap.Height; font_y++)
                    {
                        // convert red channel to alpha
                        int alpha = 255 - Font.Bitmap.GetPixel(font_x, font_y).R + temp.GetPixel(temp_x, temp_y + font_y).A;
                        alpha = (alpha > 255) ? 255 : alpha;

                        // foreground color with new alpha channel
                        temp.SetPixel(temp_x, temp_y + font_y, Color.FromArgb(alpha, FontColor));

                        // add last pixel to "border" array if the alpha greater than overlay treshold
                        if (alpha >= 255 - Font.Treshold)
                        {
                            lastPosition[temp_y + font_y] = temp_x;
                        }
                    }

                    temp_x++;
                }
            }

            // canvas start positions
            int start_x, start_y;
            // canvas end postions
            int end_x, end_y;

            if (temp_x >= bitmap.Width) // prepare to cut temp image on x axis
            {
                start_x = 0;
                end_x = bitmap.Width - 1;
                temp_x = (temp_x - end_x) / 2;
            }
            else // prepare to center temp image on x axis
            {
                start_x = (bitmap.Width - 1 - temp_x) / 2;
                end_x = start_x + temp_x - 1;
                temp_x = 0;
            }

            if (temp.Height > bitmap.Height) // prepare to cut temp image on y axis
            {
                start_y = 0;
                end_y = bitmap.Height - 1;
                temp_y = (temp.Height - bitmap.Height) / 2;
            }
            else // prepare to center temp image on y axis
            {
                start_y = (bitmap.Height - temp.Height) / 2;
                end_y = start_y + temp.Height - 1;
                temp_y = 0;
            }

            int prev_y = temp_y;

            // copy selected pixels from the temp image to the target bitmap
            for (image_x = start_x; image_x <= end_x; image_x++, temp_x++)
            {
                temp_y = prev_y;

                for (image_y = start_y; image_y <= end_y; image_y++, temp_y++)
                {
                    Color tempPixel = temp.GetPixel(temp_x, temp_y);

                    if (tempPixel.A == 0)
                    {
                        continue;
                    }

                    Color canvasPixel = bitmap.GetPixel(image_x, image_y);

                    double transparency = tempPixel.A / 255.0;

                    // alpha compositing
                    int red = (int) (tempPixel.R * transparency + canvasPixel.R * (1.0 - transparency));
                    int green = (int) (tempPixel.G * transparency + canvasPixel.G * (1.0 - transparency));
                    int blue = (int) (tempPixel.B * transparency + canvasPixel.B * (1.0 - transparency));

                    // replace pixel color
                    bitmap.SetPixel(image_x, image_y, Color.FromArgb(red, green, blue));
                }
            }

            return bitmap;
        }
    }
}
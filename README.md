# NCaptcha (.NET CAPTCHA)

## License:
MIT, for more details see LICENSE file.

## Example:

```
Captcha captcha;

// simple way

captcha = new Captcha(new {
    width = 100, // image width; pixels
    height = 50, // height; pixels
    foreground = "black", // font color; html color (#RRGGBB) or System.Drawing.Color
    background = Color.White, // background color; html color (#RRGGBB) or System.Drawing.Color
    keylength = 5, // length of key
    waves = true, // enable waves filter (distortions)
    overlay = true // enable overlaying
});

// use builder (more control)

captcha = new Captcha.Builder()
    .Background(myBackground)
    .Keygen(myKeygen)
    .Drawer(myDrawer)
    .Filters(myFilters)
    .Build();

// access key and image

var key = captcha.Key;
var image = captcha.Image;

```
# NCaptcha (.NET CAPTCHA)

## License
MIT, for more details see LICENSE file.

## Example

```csharp
Captcha captcha;

// simple way

captcha = new Captcha(new {
    Width = 100, // image width in pixels
    Height = 50, // image height in pixels
    Foreground = "black", // font color; html color (#RRGGBB) or System.Drawing.Color
    Background = Color.White, // background color; html color (#RRGGBB) or System.Drawing.Color
    KeyLength = 5, // key length
    Waves = true, // enable waves filter (distortions)
    Overlay = true // enable overlaying
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

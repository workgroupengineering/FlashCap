# FlashCap

![FlashCap](Images/FlashCap.100.png)

FlashCap - Independent camera capture library.

[![Project Status: WIP – Initial development is in progress, but there has not yet been a stable, usable release suitable for the public.](https://www.repostatus.org/badges/latest/wip.svg)](https://www.repostatus.org/#wip)

## NuGet

|Package|NuGet|
|:--|:--|
|FlashCap|[![NuGet FlashCap](https://img.shields.io/nuget/v/FlashCap.svg?style=flat)](https://www.nuget.org/packages/FlashCap)|

## CI

|main|develop|
|:--|:--|
|[![FlashCap CI build (main)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=main)](https://github.com/kekyo/FlashCap/actions?query=branch%3Amain)|[![FlashCap CI build (develop)](https://github.com/kekyo/FlashCap/workflows/.NET/badge.svg?branch=develop)](https://github.com/kekyo/FlashCap/actions?query=branch%3Adevelop)|

---

## What is this?

This is a simple camera image capture library.
By specializing only image capture, it is simple and has no any other library dependencies, [see NuGet summary page.](https://www.nuget.org/packages/FlashCap)

.NET platforms supported are as follows:

* .NET 6, 5 (net6.0, net5.0)
* .NET Core 3.1, 3.0, 2.2, 2.1, 2.0 (netcoreapp3.1 and etc)
* .NET Standard 2.1, 2.0, 1.1 (netstandard2.1 and etc)
* .NET Framework 4.8, 4.6.1, 4.5, 4.0, 3.5, 2.0 (net45 and etc)

Platforms on which camera devices can be used:

* Windows (around Video For Windows API)

TODO:

* Windows (DirectShow API)
* Linux (V2L2 API)

---

## How to use

Enumerate target devices and video characteristics:

```csharp
using FlashCap;

// Capture device enumeration:
var devices = new CaptureDevices();

foreach (var descriptor in devices.EnumerateDescriptors())
{
    // "Logicool Webcam C930e: DirectShow device, Characteristics=34"
    // "Default: VideoForWindows default, Characteristics=1"
    Console.WriteLine(descriptor);

    foreach (var characteristics in descriptor.Characteristics)
    {
        // "1920x1080 [MJPG, 24bits, 30fps]"
        // "640x480 [YUY2, 16bits, 60fps]"
        Console.WriteLine(characteristics);
    }
}
```

Then, do capture:

```csharp
// Open a device with a video characteristics:
var descriptor0 = devices.EnumerateDescriptors().ElementAt(0);

using var device = descriptor0.Open(
    descriptor0.Characteristics[0])

// Reserved pixel buffer:
var buffer = new PixelBuffer();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Capture a frame into pixel buffer:
    device.Capture(e, buffer);

    // Get image container:
    byte[] image = buffer.ExtractImage();

    // Anything use it:
    var ms = new MemoryStream(image);
    var bitmap = Bitmap.FromStream(ms);

    // ...
};

// Start processing:
device.Start();

// ...
Console.ReadLine();

// Stop processing:
device.Stop();
```

Full sample code and another library combination usage are here:

* [Windows Forms application](samples/FlashCap.WindowsForms/)

TODO:

* [WPF application](samples/FlashCap.WPF/)
* [Avalonia](samples/FlashCap.Avalonia/)

---

## Advanced: About pixel buffer

Pixel buffer (`PixelBuffer` class) is controlled about
image data allocation and buffering.
You can efficiently handle one frame of image data
by using different instances of `PixelBuffer`.

For example, frames that come in one after another can be captured
(`ICaptureDevice.Capture` method) and queued in separate pixel buffers,
and the retrieval operation (`PixelBuffer.ExtractImage` method)
can be performed in a separate thread.

This method minimizes the cost of frame arrival events and avoids frame dropping.

There is one more important feature that is relevant.
When calling `ExtractImage`, it automatically converts from unique image format
used by the imaging device to `RGB DIB` format.

For example, many image capture devices return frame data in
formats such as `YUY2` or `UYVY`, but these formats are not common.

The conversion code is multi-threaded for fast conversion.
However, in order to offload as much as possible,
the conversion is performed when the `PixelBuffer.ExtractImage` method is called.

Therefore, the following method is recommended:

1. `device.Capture(e, buffer)` is handled in the `FrameArrived` event.
2. When the image is actually needed, use `buffer.ExtractImage` to extract the image data.
This operation can be performed in a separate thread.

### Enable queuing

This is illustrated for it strategy:

```csharp
using System.Collections.Concurrent;
using SkiaSharp;

// Pixel buffer queue:
var queue = new BlockingCollection<PixelBuffer>();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Capture a frame into a pixel buffer.
    // We have to do capturing on only FrameArrived event context.
    var buffer = new PixelBuffer();
    device.Capture(e, buffer);

    // Enqueue pixel buffer.
    queue.Add(buffer);
};

// Decoding with offloaded thread:
Task.Run(() =>
{
    foreach (var buffer in queue.GetConsumingEnumerable())
    {
        // Get image container:
        byte[] image = buffer.ExtractImage();

        // Decode by SkiaSharp:
        var bitmap = SKBitmap.Decode(image);

        // (Anything use of it...)
    }
});
```

### More optimization

We can reuse `PixelBuffer` instance when it is not needed.
These code completes reusing:

```csharp
// Pixel buffer queue and reserver:
var reserver = new ConcurrentStack<PixelBuffer>();
var queue = new BlockingCollection<PixelBuffer>();

// Hook frame arrived event:
device.FrameArrived += (s, e) =>
{
    // Try despence a pixel buffer:
    if (!reserver.TryPop(out var buffer))
    {
        // If empty, create now:
        buffer = new PixelBuffer();
    }

    // Capture a frame into a dispensed pixel buffer.
    device.Capture(e, buffer);

    // Enqueue pixel buffer.
    queue.Add(buffer);
};

// Decoding with offloaded thread:
Task.Run(() =>
{
    foreach (var buffer in queue.GetConsumingEnumerable())
    {
        // Get image container:
        byte[] image = buffer.ExtractImage();

        // Now, the pixel buffer isn't needed.
        // So we can push it into reserver.
        reserver.Push(buffer);

        // Decode by SkiaSharp:
        var bitmap = SKBitmap.Decode(image);

        // (Anything use of it...)
    }
});
```

---

## License

Apache-v2.


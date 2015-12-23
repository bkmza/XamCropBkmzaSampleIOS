using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CoreGraphics;
using ObjCRuntime;
using UIKit;
using CoreImage;

namespace XamCropBkmzaSample
{
   public static class UIImageExtensions
   {
      struct vImage_Buffer
      {
         public IntPtr data;
         public uint height;
         public uint width;
         public uint rowBytes;
      };

      public static UIImage ApplySepiaFilter(this UIImage image)
      {
         var ciimage = new CIImage(image);
         var hueAdjust = new CIHueAdjust();   // first filter
         hueAdjust.Image = ciimage;
         hueAdjust.Angle = 2.094f;
         var sepia = new CISepiaTone();       // second filter
         sepia.Image = hueAdjust.OutputImage; // output from last filter, input to this one
         sepia.Intensity = 0.3f;
         CIFilter color = new CIColorControls() { // third filter
            Saturation = 2,
            Brightness = 1,
            Contrast = 3,
            Image = sepia.OutputImage    // output from last filter, input to this one
         };
         var output = color.OutputImage;
         var context = CIContext.FromOptions(null);
         // ONLY when CreateCGImage is called do all the effects get rendered
         var cgimage = context.CreateCGImage (output, output.Extent);
         var ui = UIImage.FromImage (cgimage);

         return ui;
      }

      public static UIImage ApplyFilter(this UIImage image, float brightness, float saturation, float contrast)
      {
         var colorCtrls = new CIColorControls { Image = CIImage.FromCGImage (image.CGImage) };
         var context = CIContext.FromOptions (null);
         // set the values
         colorCtrls.Brightness = brightness;
         colorCtrls.Saturation = saturation;
         colorCtrls.Contrast = contrast;
         // do the transformation
         var outputImage = colorCtrls.OutputImage;
         var result = context.CreateCGImage (outputImage, outputImage.Extent);
         return UIImage.FromImage (result);
      }

      [DllImport (Constants.AccelerateImageLibrary)]
      static extern int vImageBoxConvolve_ARGB8888 (ref vImage_Buffer src, ref vImage_Buffer dest, IntPtr tempBuffer, uint srcOffsetToROI_X, uint srcOffsetToROI_Y, uint kernel_height, uint kernel_width, byte[] backgroundColor, uint flags);

      [DllImport (Constants.AccelerateImageLibrary)]
      static extern int vImageMatrixMultiply_ARGB8888 (ref vImage_Buffer src, ref vImage_Buffer dest, short[] matrix, int divisor, IntPtr pre_bias, IntPtr post_bias, uint flags);

      public static UIImage ApplyLightEffect (this UIImage image)
      {
         UIColor tintColor = UIColor.FromWhiteAlpha (1.0f, 0.3f);
         return ApplyBlur (image, 30f, tintColor, 1.8f, null);
      }

      public static UIImage ApplyExtraLightEffect (this UIImage image)
      {
         UIColor tintColor = UIColor.FromWhiteAlpha (0.97f, 0.82f);
         return ApplyBlur (image, 20f, tintColor, 1.8f, null);
      }

      public static UIImage ApplyDarkEffect (this UIImage image)
      {
         UIColor tintColor = UIColor.FromWhiteAlpha (0.11f, 0.73f);
         return ApplyBlur (image, 20f, tintColor, 1.8f, null);
      }

      public static UIImage ApplyTintEffectWithColor (this UIImage image, UIColor tintColor)
      {
         const float EFFECT_COLOR_ALPHA = 0.6f;
         UIColor effectColor = tintColor;
         nint componentCount = tintColor.CGColor.NumberOfComponents;
         if (componentCount == 2)
         {
            nfloat b, a;
            if (tintColor.GetWhite (out b, out a))
            {
               effectColor = UIColor.FromWhiteAlpha (b, EFFECT_COLOR_ALPHA);
            }
         }
         else
         {
            nfloat r, g, b, a;
            try
            {
               tintColor.GetRGBA (out r, out g, out b, out a);
               effectColor = UIColor.FromRGBA (r, g, b, EFFECT_COLOR_ALPHA);
            }
            catch
            {
            }
         }
         return ApplyBlur (image, 10f, effectColor, -1.0f, null);
      }

      public static UIImage ApplyBlur (this UIImage image, float blurRadius, UIColor tintColor, float saturationDeltaFactor, UIImage maskImage)
      {
         if (image.Size.Width < 1 || image.Size.Height < 1)
         {
            Console.WriteLine ("*** error: invalid size: ({0} x .{1}). Both dimensions must be >= 1: {2}", image.Size.Width, image.Size.Height, image);
            return null;
         }
         if (image.CGImage == null)
         {
            Console.WriteLine ("*** error: image must be backed by a CGImage: {0}", image);
            return null;
         }
         if (maskImage != null && maskImage.CGImage == null)
         {
            Console.WriteLine ("*** error: maskImage must be backed by a CGImage: {0}", maskImage);
            return null;
         }

         CGRect imageRect = new CGRect (PointF.Empty, image.Size);
         UIImage effectImage = image;

         bool hasBlur = blurRadius > float.Epsilon;
         bool hasSaturationChange = Math.Abs (saturationDeltaFactor - 1.0f) > float.Epsilon;
         if (hasBlur || hasSaturationChange)
         {
            UIGraphics.BeginImageContextWithOptions (image.Size, false, UIScreen.MainScreen.Scale);
            CGContext effectInContext = UIGraphics.GetCurrentContext ();
            effectInContext.ScaleCTM (1.0f, -1.0f);
            effectInContext.TranslateCTM (0.0f, -image.Size.Height);
            effectInContext.DrawImage (imageRect, image.CGImage);

            CGBitmapContext effectInContextAsBitmapContext = effectInContext.AsBitmapContext ();

            vImage_Buffer effectInBuffer;
            effectInBuffer.data = effectInContextAsBitmapContext.Data;
            effectInBuffer.width = (uint)effectInContextAsBitmapContext.Width;
            effectInBuffer.height = (uint)effectInContextAsBitmapContext.Height;
            effectInBuffer.rowBytes = (uint)effectInContextAsBitmapContext.BytesPerRow;

            UIGraphics.BeginImageContextWithOptions (image.Size, false, UIScreen.MainScreen.Scale);
            CGContext effectOutContext = UIGraphics.GetCurrentContext ();

            CGBitmapContext effectOutContextAsBitmapContext = effectOutContext.AsBitmapContext ();

            vImage_Buffer effectOutBuffer;
            effectOutBuffer.data = effectOutContextAsBitmapContext.Data;
            effectOutBuffer.width = (uint)effectOutContextAsBitmapContext.Width;
            effectOutBuffer.height = (uint)effectOutContextAsBitmapContext.Height;
            effectOutBuffer.rowBytes = (uint)effectOutContextAsBitmapContext.BytesPerRow;

            if (hasBlur)
            {
               nfloat inputRadius = blurRadius * UIScreen.MainScreen.Scale;
               uint radius = (uint)Math.Floor (inputRadius * 3.0 * Math.Sqrt (2 * Math.PI) / 4 + 0.5);
               if (radius % 2 != 1)
               {
                  radius += 1;
               }
               const uint kvImageEdgeExtend = 8;
               vImageBoxConvolve_ARGB8888 (ref effectInBuffer, ref effectOutBuffer, IntPtr.Zero, 0, 0, radius, radius, null, kvImageEdgeExtend);
               vImageBoxConvolve_ARGB8888 (ref effectOutBuffer, ref effectInBuffer, IntPtr.Zero, 0, 0, radius, radius, null, kvImageEdgeExtend);
               vImageBoxConvolve_ARGB8888 (ref effectInBuffer, ref effectOutBuffer, IntPtr.Zero, 0, 0, radius, radius, null, kvImageEdgeExtend);
            }
            bool effectImageBuffersAreSwapped = false;
            if (hasSaturationChange)
            {
               float s = saturationDeltaFactor;
               float[] floatingPointSaturationMatrix = new float[] {
                  0.0722f + 0.9278f * s,  0.0722f - 0.0722f * s,  0.0722f - 0.0722f * s,  0f,
                  0.7152f - 0.7152f * s,  0.7152f + 0.2848f * s,  0.7152f - 0.7152f * s,  0f,
                  0.2126f - 0.2126f * s,  0.2126f - 0.2126f * s,  0.2126f + 0.7873f * s,  0f,
                  0f,                     0f,                     0f,                     1f,
               };
               const int divisor = 256;
               uint matrixSize = (uint)floatingPointSaturationMatrix.Length;
               short[] saturationMatrix = new short[matrixSize];
               for (uint i = 0; i < matrixSize; ++i)
               {
                  saturationMatrix [i] = (short)Math.Round (floatingPointSaturationMatrix [i] * divisor);
               }
               if (hasBlur)
               {
                  const uint kvImageNoFlags = 0;
                  vImageMatrixMultiply_ARGB8888 (ref effectOutBuffer, ref effectInBuffer, saturationMatrix, divisor, IntPtr.Zero, IntPtr.Zero, kvImageNoFlags);
                  effectImageBuffersAreSwapped = true;
               }
               else
               {
                  const uint kvImageNoFlags = 0;
                  vImageMatrixMultiply_ARGB8888 (ref effectInBuffer, ref effectOutBuffer, saturationMatrix, divisor, IntPtr.Zero, IntPtr.Zero, kvImageNoFlags);
               }
            }
            if (!effectImageBuffersAreSwapped)
            {
               effectImage = UIGraphics.GetImageFromCurrentImageContext ();
            }
            UIGraphics.EndImageContext ();

            if (effectImageBuffersAreSwapped)
            {
               effectImage = UIGraphics.GetImageFromCurrentImageContext ();
            }
            UIGraphics.EndImageContext ();
         }

         UIGraphics.BeginImageContextWithOptions (image.Size, false, UIScreen.MainScreen.Scale);
         CGContext outputContext = UIGraphics.GetCurrentContext ();
         outputContext.ScaleCTM (1.0f, -1.0f);
         outputContext.TranslateCTM (0, -image.Size.Height);

         outputContext.DrawImage (imageRect, image.CGImage);

         if (hasBlur)
         {
            outputContext.SaveState ();
            if (maskImage != null)
            {
               outputContext.ClipToMask (imageRect, maskImage.CGImage);
            }
            outputContext.DrawImage (imageRect, effectImage.CGImage);
            outputContext.RestoreState ();
         }

         if (tintColor != null)
         {
            outputContext.SaveState ();
            outputContext.SetFillColor (tintColor.CGColor);
            outputContext.FillRect (imageRect);
            outputContext.RestoreState ();
         }

         UIImage outputImage = UIGraphics.GetImageFromCurrentImageContext ();
         UIGraphics.EndImageContext ();

         return outputImage;
      }

      public static CGSize ScreenSize(this UIImage image)
      {
         var scale = image.Size.Width / UIScreen.MainScreen.Bounds.Width;
         var width = image.Size.Width / scale;
         var height = image.Size.Height / scale;

         if (height > UIScreen.MainScreen.Bounds.Height)
         {
            const int tabBarHeight = 49;
            const int headerHeight = 64;
            scale = image.Size.Height / (UIScreen.MainScreen.Bounds.Height - tabBarHeight - headerHeight);
            width = image.Size.Width / scale;
            height = image.Size.Height / scale;
         }

         return new CGSize (width, height);
      }
   }
}


using System;
using CoreGraphics;
using Foundation;
using UIKit;
using CoreImage;

namespace XamCropBkmzaSample
{
   public class BTabPreviewViewController : ViewControllerBase
   {
      string _encodedImage;
      ImageClarityType _type;
      UIImageView _previewImageView;

      public BTabPreviewViewController (string encodedImage, ImageClarityType type)
      {
         _encodedImage = encodedImage;
         _type = type;
      }

      public override void ViewDidAppear (bool animated)
      {
         base.ViewDidAppear (animated);

         foreach (var view in View.Subviews)
         {
            view.RemoveFromSuperview ();
            view.Dispose ();
         }

         var bytes = Convert.FromBase64String (_encodedImage);
         var imageData = NSData.FromArray (bytes);

         var image = UIImage.LoadFromData (imageData);
         switch (_type)
         {
            case ImageClarityType.Default:

               break;
            case ImageClarityType.BlackWhite:
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
               image = ui;
               break;
            case ImageClarityType.GrayShades:
               image = image.ApplyLightEffect ();
               break;
            case ImageClarityType.Contrast:
               image = image.ApplyExtraLightEffect ();
               break;
         }

         var scaleW = image.Size.Width / UIScreen.MainScreen.Bounds.Width;

         _previewImageView = new UIImageView (new CGRect (0, 0, image.Size.Width / scaleW, image.Size.Height / scaleW)) {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = image
         };
         View.AddSubviews (_previewImageView);
      }
   }
}


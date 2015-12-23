using System;
using CoreGraphics;
using Foundation;
using UIKit;

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
            case ImageClarityType.ExtraSaturation:
               image = image.ApplyFilter (0.5f, 2, 2f);
               break;
            case ImageClarityType.ExtraContrast:
               image = image.ApplyFilter (0.5f, 0, 4);
               break;
            case ImageClarityType.ExtraBrightness:
               image = image.ApplyFilter (0.5f, 0.1f, 2);
               break;
         }

         _previewImageView = new UIImageView (new CGRect (new CGPoint (0, 0), image.ScreenSize ())) {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = image
         };
         View.AddSubviews (_previewImageView);
      }
   }
}


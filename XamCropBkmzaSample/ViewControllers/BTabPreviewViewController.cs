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

         var scaleW = image.Size.Width / UIScreen.MainScreen.Bounds.Width;

         _previewImageView = new UIImageView (new CGRect (0, 0, image.Size.Width / scaleW, image.Size.Height / scaleW)) {
            ContentMode = UIViewContentMode.ScaleAspectFit,
            Image = image
         };
         View.AddSubviews (_previewImageView);
      }
   }
}


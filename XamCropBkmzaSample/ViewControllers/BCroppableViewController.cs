using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XamCropBkmzaSample.ViewControllers
{
   public class BCroppableViewController : ViewControllerBase
   {
      public UIImageView ImageView;
      public BCroppableView CropView;
      string _encodedImage;

      public BCroppableViewController (string encodedImage)
      {
         _encodedImage = encodedImage;
      }

      public override void ViewDidLoad ()
      {
         base.ViewDidLoad ();

//         using (var image = UIImage.FromFile ("Images/IMG_0063_PORTRAIT.JPG"))
         var bytes = Convert.FromBase64String (_encodedImage);
         var imageData = NSData.FromArray (bytes);
         using (var image = UIImage.LoadFromData (imageData))
         {
            var scaleW = image.Size.Width / UIScreen.MainScreen.Bounds.Width;

            ImageView = new UIImageView (new CGRect (0, 0, image.Size.Width / scaleW, image.Size.Height / scaleW)) {
               ContentMode = UIViewContentMode.ScaleAspectFit,
               Image = image
            };
         }

         CropView = new BCroppableView (new WeakReference (this)) { Frame = ImageView.Frame };

         View.AddSubviews (ImageView, CropView);

         CropView.UseDetector ();
      }

      public UIImage GetStrippedExifImage()
      {
         UIImage image = ImageView.Image;
         return new UIImage (image.CGImage, 1, UIImageOrientation.Up);
      }
   }
}


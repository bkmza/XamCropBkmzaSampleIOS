using System;
using UIKit;
using CoreGraphics;

namespace XamCropBkmzaSample.ViewControllers
{
   public class BCroppableViewController : ViewControllerBase
   {
      public UIImageView ImageView;
      public BCroppableView CropView;

      public BCroppableViewController ()
      {
      }

      public override void ViewDidLoad ()
      {
         base.ViewDidLoad ();

         using (var image = UIImage.FromFile ("Images/test_image4.JPG"))
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
   }
}


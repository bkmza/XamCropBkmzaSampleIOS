using System;
using UIKit;
using CoreGraphics;

namespace XamCropBkmzaSample.ViewControllers
{
   public class BCroppableViewController : UIViewController
   {
      public UIImageView ImageView;
      public BCroppableView CropView;

      public BCroppableViewController ()
      {
      }

      public override void ViewWillAppear (bool animated)
      {
         base.ViewWillAppear (animated);

         // Prevent views from showing under the navigation bar
         this.EdgesForExtendedLayout = UIRectEdge.None;
      }

      public override void ViewDidLoad ()
      {
         base.ViewDidLoad ();

         View.BackgroundColor = UIColor.White;

         using (var image = UIImage.FromFile ("Images/test_image7.JPG"))
         {
            var scaleW = image.Size.Width / UIScreen.MainScreen.Bounds.Width;
            ImageView = new UIImageView (new CGRect (0, 0, UIScreen.MainScreen.Bounds.Width, image.Size.Height / scaleW)) {
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


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

      public override void ViewDidLoad ()
      {
         base.ViewDidLoad ();

         View.BackgroundColor = UIColor.White;

         using (var image = UIImage.FromFile ("Images/test_image1.jpg"))
         {
            ImageView = new UIImageView (new CGRect (0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height)) {
//               ContentMode = UIViewContentMode.ScaleAspectFit,
               Image = image
            };
         }

         CropView = new BCroppableView (new WeakReference (this)) { Frame = ImageView.Frame };

         View.AddSubviews (ImageView, CropView);
      }
   }
}


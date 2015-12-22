using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XamCropBkmzaSample.ViewControllers
{
   public class StartViewController : ViewControllerBase
   {
      UIButton _takePictureButton;

      public StartViewController ()
      {
      }

      public override void ViewWillAppear (bool animated)
      {
         base.ViewWillAppear (animated);
         UIViewController.AttemptRotationToDeviceOrientation ();
         Title = "Take Picture";
      }

      public override void ViewDidAppear (bool animated)
      {
         base.ViewDidAppear (animated);

         _takePictureButton = new UIButton (UIButtonType.RoundedRect) {
            Frame = new CGRect (UIScreen.MainScreen.Bounds.Width / 2 - 100, UIScreen.MainScreen.Bounds.Height / 2 - 50, 200, 100),
            BackgroundColor = UIColor.Orange,
            TintColor = UIColor.Black
         };
         _takePictureButton.SetTitle ("Take Picture", UIControlState.Normal);
         _takePictureButton.Layer.CornerRadius = 5;
         View.Add (_takePictureButton);

         _takePictureButton.AddGestureRecognizer (new UITapGestureRecognizer (() =>
         {
            Camera.TakePicture (new WeakReference (this), (info) =>
            {
               UIImage initialImage = info.ValueForKey (UIImagePickerController.OriginalImage) as UIImage;
//               NSData data = initialImage.AsPNG ();
//               UIImage tempImage = new UIImage (data);
//               UIImage fixedOrientationImage = new UIImage (tempImage.CGImage, initialImage.CurrentScale, initialImage.Orientation);
//               initialImage = fixedOrientationImage;

               NSData imageData = initialImage.AsJPEG ();
               string encodedImage = imageData.GetBase64EncodedData (NSDataBase64EncodingOptions.None).ToString ();

               NavigationController.ShowViewController (new BCroppableViewController (encodedImage), this);
            });
         }));
      }
   }
}


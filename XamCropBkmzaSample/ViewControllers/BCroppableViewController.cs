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

//         using (var image = UIImage.FromFile ("Images/IMG_0066_PORTRAIT.JPG"))
//         using (var orientedImage = FixOrientation (image))
         var bytes = Convert.FromBase64String (_encodedImage);
         var imageData = NSData.FromArray (bytes);
         using (var image = UIImage.LoadFromData (imageData))
         using (var orientedImage = FixOrientation (image))
         {
            ImageView = new UIImageView (new CGRect (new CGPoint (0, 0), orientedImage.ScreenSize ())) {
               ContentMode = UIViewContentMode.ScaleAspectFit,
               Image = orientedImage
            };
         }

         CropView = new BCroppableView (new WeakReference (this)) { Frame = ImageView.Frame };

         View.AddSubviews (ImageView, CropView);

         CropView.UseDetector ();
      }

      UIImage FixOrientation (UIImage image)
      {
         if (image.Orientation == UIImageOrientation.Up)
            return image;

         CGAffineTransform transform = CGAffineTransform.MakeIdentity ();

         switch (image.Orientation)
         {
            case UIImageOrientation.Down:
            case UIImageOrientation.DownMirrored:
               transform = CGAffineTransform.Translate (transform, image.Size.Width, image.Size.Height);
               transform = CGAffineTransform.Rotate (transform, (nfloat)(Math.PI));
               break;
            case UIImageOrientation.Left:
            case UIImageOrientation.LeftMirrored:
               transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
               transform = CGAffineTransform.Rotate (transform, (nfloat)(Math.PI / 2));
               break;
            case UIImageOrientation.Right:
            case UIImageOrientation.RightMirrored:
               transform = CGAffineTransform.Translate (transform, 0, image.Size.Height);
               transform = CGAffineTransform.Rotate (transform, (nfloat)(-1 * Math.PI / 2));
               break;
            case UIImageOrientation.Up:
            case UIImageOrientation.UpMirrored:
               break;
         }

         switch (image.Orientation)
         {
            case UIImageOrientation.UpMirrored:
            case UIImageOrientation.DownMirrored:
               transform = CGAffineTransform.Translate (transform, image.Size.Width, 0);
               transform = CGAffineTransform.Scale (transform, -1, 1);
               break;
            case UIImageOrientation.LeftMirrored:
            case UIImageOrientation.RightMirrored:
               transform = CGAffineTransform.Translate (transform, image.Size.Height, 0);
               transform = CGAffineTransform.Scale (transform, -1, 1);
               break;
            case UIImageOrientation.Up:
            case UIImageOrientation.Down:
            case UIImageOrientation.Left:
            case UIImageOrientation.Right:
               break;
         }

         using (CGBitmapContext ctx = new CGBitmapContext (null, (nint)image.Size.Width, (nint)image.Size.Height, (nint)image.CGImage.BitsPerComponent, (nint)0, 
                                         image.CGImage.ColorSpace, image.CGImage.BitmapInfo))
         {
            ctx.ConcatCTM (transform);
            switch (image.Orientation)
            {
               case UIImageOrientation.Left:
               case UIImageOrientation.LeftMirrored:
               case UIImageOrientation.Right:
               case UIImageOrientation.RightMirrored:
                  ctx.DrawImage (new CGRect (0, 0, image.Size.Height, image.Size.Width), image.CGImage);
                  break;
               default:
                  ctx.DrawImage (new CGRect (0, 0, image.Size.Width, image.Size.Height), image.CGImage);
                  break;
            }

            using (var cgImage = ctx.ToImage ())
            {
               return new UIImage (cgImage);
            }
         }
      }
   }
}


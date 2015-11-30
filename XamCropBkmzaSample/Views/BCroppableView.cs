using System;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using CoreAnimation;
using XamCropBkmzaSample.ViewControllers;
using CoreImage;

namespace XamCropBkmzaSample
{
   public class BCroppableView : UIView
   {
      List<BMarkerImageView> _markers;
      bool isCroppedImageDisplayed = false;
      const float M_PI = 3.14159265358979323846264338327950288f;

      public WeakReference WeakParent;

      BCroppableViewController _parent
      {
         get
         {
            if (WeakParent == null || !WeakParent.IsAlive)
               return null;
            return WeakParent.Target as BCroppableViewController;
         }
      }

      public BCroppableView (WeakReference parentView)
      {
         WeakParent = parentView;

         BackgroundColor = UIColor.Clear;
         Opaque = false;

         Alpha = 0.7f;

         _markers = new List<BMarkerImageView> ();

         nfloat leftX = UIScreen.MainScreen.Bounds.Width * 0.1f;
         nfloat rightX = UIScreen.MainScreen.Bounds.Width - leftX;
         nfloat topY = UIScreen.MainScreen.Bounds.Height * 0.1f;
         nfloat bottomY = UIScreen.MainScreen.Bounds.Height - topY;

         AddMarker (new CGPoint (leftX, topY));
         AddMarker (new CGPoint (rightX, topY));
         AddMarker (new CGPoint (rightX, bottomY));
         AddMarker (new CGPoint (leftX, bottomY));
      }

      void AddMarker (CGPoint location)
      {
         var marker = new BMarkerImageView (new WeakReference (this), location);
         _markers.Add (marker);
         Add (marker);
      }

      public override void Draw (CGRect rect)
      {
         base.Draw (rect);
         using (CGContext g = UIGraphics.GetCurrentContext ())
         {
            if (!isCroppedImageDisplayed)
            {
               g.ClearRect (Frame);

               g.SetLineWidth (10);
               UIColor.Blue.SetFill ();
               UIColor.Red.SetStroke ();

               var path = new CGPath ();
               path.AddLines (_markers.Select (x => x.Location).ToArray ());
               path.CloseSubpath ();

               g.AddPath (path);
               g.DrawPath (CGPathDrawingMode.FillStroke);
            }
            else
            {
               g.ClearRect (Frame);
               var markers = _markers;
               foreach (var marker in markers)
               {
                  marker.RemoveFromSuperview ();
               }
            }
         }
      }

      public void SetEnhancedImage ()
      {
         var image = GetCroppedImage ();

         CIImage enhancedImage = new CIImage (image);
         var transform = new CGAffineTransform (1F, .5F, .5F, 1F, 0F, 0F);
//         transform.Rotate (-90 * (M_PI / 180));
         var affineTransform = new CIAffineTransform () {
            Image = enhancedImage,
            Transform = transform
         };
         var output = affineTransform.OutputImage;
         var context = CIContext.FromOptions (null);
         var cgimage = context.CreateCGImage (output, output.Extent);
         InvokeOnMainThread (() =>
         {
            _parent.ImageView.Image = UIImage.FromImage (output);
         });

         isCroppedImageDisplayed = true;
      }

      public void SetCroppedImage ()
      {
         var image = GetCroppedImage ();
         InvokeOnMainThread (() =>
         {
            _parent.ImageView = new UIImageView (image) {
               Frame = new CGRect (0, 0, image.Size.Width, image.Size.Height)
            };
         });
         isCroppedImageDisplayed = true;
      }

      public UIImage GetCroppedImage ()
      {
         UIImageView imageView = _parent.ImageView;

         CGSize size = imageView.Layer.Bounds.Size;
         UIGraphics.BeginImageContextWithOptions (size, false, 0.0f);
         imageView.Layer.RenderInContext (UIGraphics.GetCurrentContext ());

         UIImage image = UIGraphics.GetImageFromCurrentImageContext ();

         UIGraphics.EndImageContext ();

         return image;
      }

      public void MaskImageView ()
      {
         UIImageView imageView = _parent.ImageView;

         UIBezierPath path = GetPath ();

         CGRect rect = CGRect.Empty;
         rect.Size = new CGSize (UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);

         CAShapeLayer shapeLayer = new CAShapeLayer ();
         shapeLayer.Frame = rect;
         shapeLayer.Path = path.CGPath;
         shapeLayer.FillColor = UIColor.White.CGColor;
         shapeLayer.BackgroundColor = UIColor.Clear.CGColor;
         imageView.Layer.Mask = shapeLayer;
      }

      UIBezierPath GetPath ()
      {
         if (_markers.Count <= 0)
            return null;

         UIBezierPath path = new UIBezierPath ();

         CGPoint p1 = _markers [0].Frame.Location;
         path.MoveTo (p1);

         for (int i = 1; i < _markers.Count; i++)
         {
            CGPoint p = _markers [i].Frame.Location;
            path.AddLineTo (p);
         }
         path.ClosePath ();
         return path;
      }
   }
}


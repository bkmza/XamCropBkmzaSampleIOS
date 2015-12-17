using System;
using UIKit;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using CoreAnimation;
using XamCropBkmzaSample.ViewControllers;
using CoreImage;
using Foundation;

namespace XamCropBkmzaSample
{
   public class BCroppableView : UIView
   {
      List<BMarkerImageView> _markers;
      bool isCroppedImageDisplayed = false;
      CIDetector detector;

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

      CGPoint PreparePoint (CGPoint point)
      {
         return new CGPoint ((float)(point.X * _parent.ImageView.Image.Size.Width / UIScreen.MainScreen.Bounds.Width),
            (float)(_parent.ImageView.Image.Size.Height - (point.Y * _parent.ImageView.Image.Size.Height / UIScreen.MainScreen.Bounds.Height)));
      }

      CGPoint ConvertImageToScreen (CGPoint point)
      {
         var scaleW = _parent.ImageView.Image.Size.Width / UIScreen.MainScreen.Bounds.Width;
         return new CGPoint (
            point.X / scaleW,
            (_parent.ImageView.Frame.Height - (point.Y / _parent.ImageView.Image.Size.Height * _parent.ImageView.Frame.Height)));
      }

      public void SetEnhancedImage ()
      {
         using (CIImage ciImage = new CIImage (_parent.ImageView.Image))
         {
            InvokeOnMainThread (() =>
            {
               using (var dict = new NSMutableDictionary ())
               {
//                  var topLeft = new CGPoint (PreparePoint (_markers [0].Location));
//                  var topRight = new CGPoint (PreparePoint (_markers [1].Location));
//                  var bottomRight = new CGPoint (PreparePoint (_markers [2].Location));
//                  var bottomLeft = new CGPoint (PreparePoint (_markers [3].Location));

                  dict.Add (key: new NSString ("inputTopLeft"), value: new CIVector (_currRect.TopLeft));
                  dict.Add (key: new NSString ("inputTopRight"), value: new CIVector (_currRect.TopRight));
                  dict.Add (key: new NSString ("inputBottomLeft"), value: new CIVector (_currRect.BottomLeft));
                  dict.Add (key: new NSString ("inputBottomRight"), value: new CIVector (_currRect.BottomRight));

                  using (var perspectiveCorrectedImage = ciImage.CreateByFiltering ("CIPerspectiveCorrection", dict))
                  {
                     using (var ctx = CIContext.FromOptions (null))
                     using (CGImage convertedCGImage = ctx.CreateCGImage (perspectiveCorrectedImage, perspectiveCorrectedImage.Extent))
                     using (UIImage convertedUIImage = UIImage.FromImage (convertedCGImage))
                     {
                        _parent.ImageView.Image = convertedUIImage;
                        _parent.ImageView.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
                     }
                  }
               }
            });
         }

         isCroppedImageDisplayed = true;
      }

      CIRectangleFeature _currRect;

      public void UseDetector ()
      {
         var options = new CIDetectorOptions {
            Accuracy = FaceDetectorAccuracy.High,
            AspectRatio = 1.41f
         };

         detector = CIDetector.CreateRectangleDetector (context: null, detectorOptions: options);

         using (CIImage ciImage = new CIImage (_parent.ImageView.Image))
         {
            InvokeOnMainThread (() =>
            {
               using (var dict = new NSMutableDictionary ())
               {
                  var rectangles = detector.FeaturesInImage (ciImage);
                  if (rectangles.Length > 0)
                  {
                     _currRect = (CIRectangleFeature)rectangles [0];

                     var topLeft = ConvertImageToScreen (_currRect.TopLeft);
                     var topRight = ConvertImageToScreen (_currRect.TopRight);
                     var bottomRight = ConvertImageToScreen (_currRect.BottomRight);
                     var bottomLeft = ConvertImageToScreen (_currRect.BottomLeft);

                     dict.Add (key: new NSString ("inputTopLeft"), value: new CIVector (topLeft));
                     dict.Add (key: new NSString ("inputTopRight"), value: new CIVector (topRight));
                     dict.Add (key: new NSString ("inputBottomRight"), value: new CIVector (bottomRight));
                     dict.Add (key: new NSString ("inputBottomLeft"), value: new CIVector (bottomLeft));

                     _markers [0].Location = topLeft;
                     _markers [1].Location = topRight;
                     _markers [2].Location = bottomRight;
                     _markers [3].Location = bottomLeft;
                  }
               }
            });
         }
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


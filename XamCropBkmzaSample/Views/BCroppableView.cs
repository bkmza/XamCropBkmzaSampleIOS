﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Foundation;
using UIKit;
using XamCropBkmzaSample.ViewControllers;

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

         nfloat leftX = _parent.ImageView.Frame.Width * 0.1f;
         nfloat rightX = _parent.ImageView.Frame.Width - leftX;
         nfloat topY = _parent.ImageView.Frame.Height * 0.1f;
         nfloat bottomY = _parent.ImageView.Frame.Height - topY;

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

      nfloat _scale
      {
         get
         {
            return _parent.ImageView.Image.Size.Width / _parent.ImageView.Frame.Width;
         }
      }

      CGPoint ConvertScreenToImageCoords (CGPoint point)
      {
         var cgPoint = new CGPoint (
                          point.X * _scale,
                          _parent.ImageView.Image.Size.Height - (point.Y * _scale));
         return cgPoint;
      }

      CGPoint ConvertImageToScreenCoords (CGPoint point)
      {
         var cgPoint = new CGPoint (
                          point.X / _scale,
                          _parent.ImageView.Frame.Height - (point.Y / _scale));
         return cgPoint;
      }

      public void SetEnhancedImage ()
      {
         using (CIImage ciImage = new CIImage (_parent.ImageView.Image))
         {
            InvokeOnMainThread (() =>
            {
               using (var dict = new NSMutableDictionary ())
               {
                  var topLeft = new CGPoint (ConvertScreenToImageCoords (_markers [0].Location));
                  var topRight = new CGPoint (ConvertScreenToImageCoords (_markers [1].Location));
                  var bottomRight = new CGPoint (ConvertScreenToImageCoords (_markers [2].Location));
                  var bottomLeft = new CGPoint (ConvertScreenToImageCoords (_markers [3].Location));

                  dict.Add (key: new NSString ("inputTopLeft"), value: new CIVector (topLeft));
                  dict.Add (key: new NSString ("inputTopRight"), value: new CIVector (topRight));
                  dict.Add (key: new NSString ("inputBottomRight"), value: new CIVector (bottomRight));
                  dict.Add (key: new NSString ("inputBottomLeft"), value: new CIVector (bottomLeft));

                  using (var perspectiveCorrectedImage = ciImage.CreateByFiltering ("CIPerspectiveCorrection", dict))
                  {
                     using (var ctx = CIContext.FromOptions (null))
                     using (CGImage convertedCGImage = ctx.CreateCGImage (perspectiveCorrectedImage, perspectiveCorrectedImage.Extent))
                     using (UIImage convertedUIImage = UIImage.FromImage (convertedCGImage))
                     {
                        NSData imageData = convertedUIImage.AsJPEG ();
                        _encodedImage = imageData.GetBase64EncodedData (NSDataBase64EncodingOptions.None).ToString ();
                        _parent.ShowViewController (new BPreviewController (encodedImage: _encodedImage), this);
                     }
                  }
               }
            });
         }

         isCroppedImageDisplayed = false;
      }

      CIRectangleFeature _currRect;
      string _encodedImage;

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
                  var orient = GetExifOrientation (_parent.ImageView.Image);
                  var rectangles = detector.FeaturesInImage (ciImage, orient);
                  if (rectangles.Length > 0)
                  {
                     _currRect = (CIRectangleFeature)rectangles [0];

                     _markers [0].Location = ConvertImageToScreenCoords (_currRect.TopLeft);
                     _markers [1].Location = ConvertImageToScreenCoords (_currRect.TopRight);
                     _markers [2].Location = ConvertImageToScreenCoords (_currRect.BottomRight);
                     _markers [3].Location = ConvertImageToScreenCoords (_currRect.BottomLeft);
                  }
               }
            });
         }
      }

      CIImageOrientation GetExifOrientation (UIImage image)
      {
         CIImageOrientation orientation;
         switch (image.Orientation)
         {
            case UIImageOrientation.Up:
               orientation = CIImageOrientation.TopLeft;
               break;
            case UIImageOrientation.Down:
               orientation = CIImageOrientation.BottomRight;
               break;
            case UIImageOrientation.Left:
               orientation = CIImageOrientation.LeftBottom;
               break;
            case UIImageOrientation.Right:
               orientation = CIImageOrientation.RightTop;
               break;
            case UIImageOrientation.UpMirrored:
               orientation = CIImageOrientation.TopRight;
               break;
            case UIImageOrientation.DownMirrored:
               orientation = CIImageOrientation.BottomLeft;
               break;
            case UIImageOrientation.LeftMirrored:
               orientation = CIImageOrientation.LeftTop;
               break;
            case UIImageOrientation.RightMirrored:
               orientation = CIImageOrientation.RightBottom;
               break;
            default:
               throw new NotImplementedException ();
         }
         return orientation;
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


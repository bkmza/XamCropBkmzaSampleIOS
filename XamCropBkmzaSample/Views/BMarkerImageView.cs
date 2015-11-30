using System;
using UIKit;
using CoreGraphics;

namespace XamCropBkmzaSample
{
   public class BMarkerImageView : UIImageView
   {
      UIPanGestureRecognizer _pan;
      UITapGestureRecognizer _doubleTap;

      public WeakReference WeakParent;

      BCroppableView _parent
      {
         get
         {
            if (WeakParent == null || !WeakParent.IsAlive)
               return null;
            return WeakParent.Target as BCroppableView;
         }
      }

      public BMarkerImageView (WeakReference parentView, CGPoint location)
      {
         WeakParent = parentView;
         _location = location;

         UserInteractionEnabled = true;

         Frame = new CGRect (Location.X - 22, Location.Y - 22, 44, 44);
         using (var image = UIImage.FromFile ("Images/icon_marker.png"))
         {
            Image = image;
         }

         _pan = new UIPanGestureRecognizer (() =>
         {
            if ((_pan.State == UIGestureRecognizerState.Began || _pan.State == UIGestureRecognizerState.Changed) && (_pan.NumberOfTouches == 1))
            {
               Center = _pan.LocationInView (_parent);
               Location = Center;
               _parent.SetNeedsDisplay ();
            }
            else if (_pan.State == UIGestureRecognizerState.Ended)
            {
            }
         });

         _doubleTap = new UITapGestureRecognizer ((gesture) => Crop ()) { 
            NumberOfTapsRequired = 2, NumberOfTouchesRequired = 1 
         };

         AddGestureRecognizer (_pan);
         AddGestureRecognizer (_doubleTap);
      }

      void Crop ()
      {
         _parent.MaskImageView ();
         _parent.SetCroppedImage ();
         _parent.SetNeedsDisplay ();
      }

      public CGPoint Location
      {
         get
         {
            return _location;
         }

         set
         {
            _location = value;
            SetNeedsDisplay ();
         }
      }

      CGPoint _location;
   }
}


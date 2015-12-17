using System;
using UIKit;

namespace XamCropBkmzaSample
{
   public class ViewControllerBase : UIViewController
   {
      public ViewControllerBase ()
      {
      }

      public ViewControllerBase (IntPtr handle) : base (handle)
      {
      }

      public override void ViewWillAppear (bool animated)
      {
         base.ViewWillAppear (animated);

         // Prevent views from showing under the navigation bar
         EdgesForExtendedLayout = UIRectEdge.None;
         View.BackgroundColor = UIColor.White;
      }
   }
}


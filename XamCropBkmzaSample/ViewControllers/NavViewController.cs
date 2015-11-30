using System;
using UIKit;

namespace XamCropBkmzaSample.ViewControllers
{
   public class NavViewController : UINavigationController
   {
      public NavViewController ()
         :base (new BCroppableViewController())
      {

      }
   }
}


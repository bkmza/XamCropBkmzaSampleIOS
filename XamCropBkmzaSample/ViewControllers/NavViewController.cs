using UIKit;

namespace XamCropBkmzaSample.ViewControllers
{
   public class NavViewController : UINavigationController
   {
      public NavViewController ()
         :base (new StartViewController())
//         :base (new BCroppableViewController(null))
      {

      }
   }
}


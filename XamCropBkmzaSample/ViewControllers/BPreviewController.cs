using System;
using Foundation;
using UIKit;

namespace XamCropBkmzaSample
{
   public class BPreviewController : TabBarControllerBase
   {
      string _encodedImage;
      UIViewController _tab1, _tab2, _tab3, _tab4;

      public BPreviewController (string encodedImage)
      {
         _encodedImage = encodedImage;
      }

      public override void ViewDidAppear (bool animated)
      {
         base.ViewDidAppear (animated);

         var bytes = Convert.FromBase64String (_encodedImage);
         var imageData = NSData.FromArray (bytes);

         using (var image = UIImage.LoadFromData (imageData))
         {
            Title = string.Format ("Image size: {0} x {1}", image.Size.Width, image.Size.Height);
         }

         _tab1 = new BTabPreviewViewController (_encodedImage, ImageClarityType.Default) {
            Title = "Default",
            TabBarItem = new UITabBarItem ("Default", UIImage.FromBundle ("Images/icon_tab.png"), 0)
         };

         _tab2 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraSaturation) {
            Title = "BlackWhite",
            TabBarItem = new UITabBarItem ("Saturation", UIImage.FromBundle ("Images/icon_tab.png"), 1)
         };

         _tab3 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraContrast) {
            Title = "GrayShades",
            TabBarItem = new UITabBarItem ("Contrast", UIImage.FromBundle ("Images/icon_tab.png"), 2)
         };

         _tab4 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraBrightness) {
            Title = "Contrast",
            TabBarItem = new UITabBarItem ("Brightness", UIImage.FromBundle ("Images/icon_tab.png"), 3)
         };

         ViewControllers = new[] { _tab1, _tab2, _tab3, _tab4 };
      }
   }
}


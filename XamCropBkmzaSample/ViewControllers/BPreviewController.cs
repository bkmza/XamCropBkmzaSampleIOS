using System;
using Foundation;
using UIKit;
using CoreGraphics;

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
            TabBarItem = new UITabBarItem (UITabBarSystemItem.Bookmarks, 0),
         };

         _tab2 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraSaturation) {
            Title = "BlackWhite",
            TabBarItem = new UITabBarItem (UITabBarSystemItem.Contacts, 1)
         };

         _tab3 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraContrast) {
            Title = "GrayShades",
            TabBarItem = new UITabBarItem (UITabBarSystemItem.Downloads, 2)
         };

         _tab4 = new BTabPreviewViewController (_encodedImage, ImageClarityType.ExtraBrightness) {
            Title = "Contrast",
            TabBarItem = new UITabBarItem (UITabBarSystemItem.Favorites, 3)
         };

         ViewControllers = new[] { _tab1, _tab2, _tab3, _tab4 };
      }
   }
}


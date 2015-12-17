using System;
using CoreGraphics;

namespace XamCropBkmzaSample
{
   public static class CGPointExtension
   {
      public static CGPoint PointToCGPoint(this Point point)
      {
         return new CGPoint(point.X, point.Y);
      }
   }
}


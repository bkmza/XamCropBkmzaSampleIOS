using System;
using System.Collections.Generic;
using System.Linq;

namespace XamCropBkmzaSample
{
   public class Point
   {
      public float X;
      public float Y;

      public Point (float x, float y)
      {
         X = x;
         Y = y;
      }
   }

   public static class PointExtension
   {
      public static Point TopLeft<T> (this List<T> pointList) where T: Point
      {
         if (pointList == null || !pointList.Any ())
         {
            throw new Exception ();
         }

         Point p = pointList.OrderBy (x => x.X).ThenByDescending (x => x.Y).First ();

         return p;
      }

      public static Point TopRight<T> (this List<T> pointList) where T: Point
      {
         if (pointList == null || !pointList.Any ())
         {
            throw new Exception ();
         }

         Point p = pointList.OrderByDescending (x => x.X).ThenByDescending (x => x.Y).First ();

         return p;
      }

      public static Point BottomRight<T> (this List<T> pointList) where T: Point
      {
         if (pointList == null || !pointList.Any ())
         {
            throw new Exception ();
         }

         Point p = pointList.OrderByDescending (x => x.X).ThenBy (x => x.Y).First ();

         return p;
      }

      public static Point BottomLeft<T> (this List<T> pointList) where T: Point
      {
         if (pointList == null || !pointList.Any ())
         {
            throw new Exception ();
         }

         Point p = pointList.OrderBy (x => x.X).ThenBy (x => x.Y).First ();

         return p;
      }
   }
}


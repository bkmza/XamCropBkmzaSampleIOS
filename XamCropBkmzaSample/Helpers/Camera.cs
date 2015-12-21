//
// Camera.cs: Support code for taking pictures
//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using UIKit;
using Foundation;

namespace XamCropBkmzaSample
{
   public static class Camera
   {
      static UIImagePickerController picker;
      static Action<NSDictionary> _callback;

      static void Init ()
      {
         if (picker != null)
            return;

         picker = new UIImagePickerController ();
         picker.Delegate = new CameraDelegate ();
      }

      class CameraDelegate : UIImagePickerControllerDelegate
      {
         public override void FinishedPickingMedia (UIImagePickerController picker, NSDictionary info)
         {
            var cb = _callback;
            _callback = null;

            picker.DismissModalViewController (true);
            if (cb != null)
            {
               cb (info);
            }
         }
      }

      static WeakReference _weakParent;
      static UIViewController Parent
      {
         get
         {
            if (_weakParent ==null || !_weakParent.IsAlive)
               return null;
            return _weakParent.Target as UIViewController;
         }
      }

      public static void TakePicture (WeakReference parent, Action<NSDictionary> callback)
      {
         _weakParent = parent;

         Init ();
         picker.SourceType = UIImagePickerControllerSourceType.Camera;

         _callback = callback;
         Parent.PresentModalViewController (picker, true);
      }
   }
}

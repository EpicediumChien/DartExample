using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace nDragElement
{
    public class DragElementHandler
    {
        private FrameworkElement DragElement;
        private FrameworkElement ContainerElement;

        //Setup DragElement and DragContainer
        public void Init(FrameworkElement dragElement, FrameworkElement containerElement)
        {
            DragElement = dragElement;
            ContainerElement = containerElement;

            DragElement.MouseDown += DragElement_MouseDown;
            DragElement.MouseMove += DragElement_MouseMove;
            DragElement.MouseUp += DragElement_MouseUp;
        }

        //Called when UI layout is changed
        private bool isLayoutChanged = true;

        //Known Issue: wrong deltaX deltaY calculation need to be fixed
        public void RefreshLayout()
        {
            if (ContainerElement == null)
                return;

            //Get the center point of Container (relate to the Container)
            Point ptContainerCenter = new Point(ContainerElement.ActualWidth / 2, ContainerElement.ActualHeight / 2);
            //Get the center point of Drag element (relate to the Container)
            //
            //1. Get the Center point of DragElement (relate to the DragElement)
            Point ptCenterElement = new Point(0, 0);
                //new Point(DragElement.ActualWidth / 2, DragElement.ActualHeight / 2);
            //2. Translate ptCenterElement to "relate to Container"
            Point ptDragCenter = DragElement.TranslatePoint(ptCenterElement, ContainerElement);

            //Calculate the delta X,Y between "ptContainerCenter" and "ptDragCenter"
            double deltaX = ptDragCenter.X - ptContainerCenter.X;
            double deltaY = ptDragCenter.Y - ptContainerCenter.Y;

            //Update to the DragElement.RenderTransformation
            TranslateTransform translateTransform = new TranslateTransform()
            {
                X = deltaX,
                Y = deltaY
            };
            DragElement.RenderTransform = translateTransform;
            isLayoutChanged = true;
        }

        public void Reset()
        {
            if (DragElement == null)
                return;

            DragElement.RenderTransform = new TranslateTransform() 
            {
                X = 0,
                Y = 0
            };
            isLayoutChanged = true;
        }

        private Point ptMouseDown;
        private Rect containerRange;

        private void DragElement_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isLayoutChanged)
            {
                //Get the position (relate to Container) of mouse down
                ptMouseDown = Mouse.GetPosition(ContainerElement);

                //Determine the range of Container, which the dragElement can be moved to
                containerRange = new Rect
                (
                    -1.00 * ContainerElement.ActualWidth / 2.00,
                    -1.00 * ContainerElement.ActualHeight / 2.00,
                    ContainerElement.ActualWidth,
                    ContainerElement.ActualHeight
                );

                //Clear the flag, until Layout changed again
                isLayoutChanged = false;
            }

            //Start capture mouse
            DragElement.CaptureMouse();
        }
        private void DragElement_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!DragElement.IsMouseCaptured)
                return;

            //Get the new position (relate to Container) of mouse
            Point ptMove = Mouse.GetPosition(ContainerElement);

            //Calculate the delta of mouse movement
            double deltaX = ptMove.X - ptMouseDown.X;
            double deltaY = ptMove.Y - ptMouseDown.Y;

            //Limit the new position inside Container range
            deltaX = Math.Max(deltaX, containerRange.Left);
            deltaX = Math.Min(deltaX, containerRange.Right);
            deltaY = Math.Max(deltaY, containerRange.Top);
            deltaY = Math.Min(deltaY, containerRange.Bottom);

            TranslateTransform translateTransform = DragElement.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform()
                {
                    X = deltaX,
                    Y = deltaY
                };
            }
            else
            {
                translateTransform.X = deltaX;
                translateTransform.Y = deltaY;
            }

            //Apply the TranslateTransform to the DragElement to move to new position
            //TranslateTransform translateTransform = new TranslateTransform()
            //{
            //    X = deltaX,
            //    Y = deltaY
            //};
            DragElement.RenderTransform = translateTransform;
        }

        private void DragElement_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //End of Capture
            DragElement.ReleaseMouseCapture();
        }

 
    }
}

﻿using Autodesk.DesignScript.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;

namespace Bloodstone
{
    public partial class BloodstoneControl : UserControl
    {
        private enum CursorIndex
        {
            Pointer, Hand, HandPan, HandDrag
        }

        private Point prevMousePosition = new Point();
        private SynthesizedGraph currentGraph = null;
        private VisualizerHwndHost visualizer = null;
        private GraphVisualHost graphVisualHost = null;
        private SliderVisualHost sliderVisualHost = null;

        // Cursor resources.
        private Cursor[] canvasCursors = null;

        #region Public Operational Class Methods

        public BloodstoneControl()
        {
            InitializeComponent();
            this.Loaded += OnBloodstoneControlLoaded;
        }

        public ISynthesizedGraph GetSynthesizedGraph()
        {
            return new SynthesizedGraph();
        }

        public void SetSynthesizedGraph(ISynthesizedGraph graph)
        {
            var nextGraph = graph as SynthesizedGraph;
            nextGraph.BuildGraphStructure();

            if (this.currentGraph != null)
            {
                var removedNodes = this.currentGraph.NodesNotInGraph(nextGraph);
                if (removedNodes.Any())
                {
                    if (visualizer != null && (visualizer.CurrentVisualizer != null))
                        visualizer.CurrentVisualizer.RemoveNodeGeometries(removedNodes);
                }
            }

            this.currentGraph = nextGraph;
            this.graphVisualHost.RefreshGraph(nextGraph);
        }

        public void SetNodeGeometries(Dictionary<string, IRenderPackage> geometries)
        {
            if (visualizer != null && (visualizer.CurrentVisualizer != null))
            {
                var depths = this.currentGraph.GetNodeDepths();
                var colors = this.currentGraph.GetNodeColors();
                var details = new Dictionary<string, NodeDetails>();

                foreach (var depth in depths)
                {
                    IRenderPackage geometry = null;
                    geometries.TryGetValue(depth.Key, out geometry);
                    details.Add(depth.Key, new NodeDetails(depth.Value, geometry));
                }

                double inv = 1.0 / 255.0;
                foreach (var color in colors)
                {
                    var c = color.Value;
                    var nodeDetails = details[color.Key];
                    nodeDetails.SetColor(c.R * inv, c.G * inv, c.B * inv);
                }

                visualizer.CurrentVisualizer.UpdateNodeDetails(details);
            }
        }

        #endregion

        #region Internal Class Methods

        internal void DestroyVisualizer()
        {
            if (visualizer != null)
            {
                visualizer.Dispose();
                visualizer = null;
            }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnBloodstoneControlLoaded(object sender, RoutedEventArgs e)
        {
            this.graphVisualHost = new GraphVisualHost();
            this.graphCanvas.Children.Add(graphVisualHost);
            this.graphCanvas.MouseDown += OnGraphCanvasMouseDown;
            this.graphCanvas.MouseUp += OnGraphCanvasMouseUp;
            this.graphCanvas.MouseMove += OnGraphCanvasMouseMove;

            this.sliderVisualHost = new SliderVisualHost(canvasScrollViewer);
            this.sliderCanvas.Children.Add(sliderVisualHost);
            this.sliderVisualHost.Changed += delegate(object s, SliderEventArgs se)
            {
                if (visualizer != null && (visualizer.CurrentVisualizer != null))
                    visualizer.CurrentVisualizer.BlendGeometryLevels((float)se.Value);
            };

            if (visualizer == null)
            {
                var b = VisualizerHostElement;
                visualizer = new VisualizerHwndHost(b.ActualWidth, b.ActualHeight);
                VisualizerHostElement.Child = visualizer;
            }

            ActivateCusor(CursorIndex.HandPan);
        }

        private void OnGraphCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Always get the position before "CaptureMouse" because once the 
            // mouse is captured, it immediately sends "MouseMove" to the canvas,
            // so if the position is to be used in "MouseMove", it had better be 
            // correct at that point.
            // 
            this.prevMousePosition = e.GetPosition(this);
            if (this.graphCanvas.CaptureMouse())
                ActivateCusor(CursorIndex.HandDrag);
        }

        private void OnGraphCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            ActivateCusor(CursorIndex.HandPan);
            if (this.graphCanvas.IsMouseCaptured)
                this.graphCanvas.ReleaseMouseCapture();
        }

        private void OnGraphCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (this.graphCanvas.IsMouseCaptured == false)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var position = e.GetPosition(this);
            var delta = position - prevMousePosition;

            double vertOffset = this.canvasScrollViewer.VerticalOffset;
            double horzOffset = this.canvasScrollViewer.HorizontalOffset;
            this.canvasScrollViewer.ScrollToVerticalOffset(vertOffset - delta.Y);
            this.canvasScrollViewer.ScrollToHorizontalOffset(horzOffset - delta.X);

            // Update the current mouse position.
            this.prevMousePosition = position;
        }

        #endregion

        #region Private Class Helper Methods

        private Cursor LoadCursorResource(string name)
        {
            var baseUri = "pack://application:,,,/Bloodstone.Net;component/Resources";
            Uri uri = new Uri(string.Format("{0}/Cursors/{1}", baseUri, name));
            StreamResourceInfo cursorStream = Application.GetResourceStream(uri);
            return new Cursor(cursorStream.Stream);
        }

        private void ActivateCusor(CursorIndex cursorIndex)
        {
            if (this.canvasCursors == null)
            {
                canvasCursors = new Cursor[]
                {
                    Cursors.Arrow,
                    LoadCursorResource("hand.cur"),
                    LoadCursorResource("hand_pan.cur"),
                    LoadCursorResource("hand_drag.cur"),
                };
            }

            this.graphCanvas.Cursor = canvasCursors[(int)cursorIndex];
        }

        #endregion
    }
}
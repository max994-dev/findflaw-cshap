using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using FindFlaw.Models;
using HelixToolkit.Wpf;

namespace FindFlaw.Managers
{
    public class LineManager
    {
        private HelixViewport3D viewport;
        // Lines created
        private List<LineMarker> lines;

        private LineMarker selectedLine;


        private Color currentColor = Colors.Red;

        private int nextId = 1;

        public int Count => lines.Count;
        public bool HasSelection => selectedLine != null;

        public IEnumerable<LineMarker> Lines => lines;

        private DispatcherTimer sphereGrowTimer;
        private LineMarker growingLine;
        private double maxSphereRadius = 100;  // tune


        private void EnsureSphereTimer()
        {
            if (sphereGrowTimer != null) return;

            sphereGrowTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // grow speed
            };
            sphereGrowTimer.Tick += SphereGrowTimer_Tick;
        }

        private void SphereGrowTimer_Tick(object? sender, EventArgs e)
        {
            if (growingLine?.SphereVisual == null) return;

            double r = growingLine.SphereVisual.Radius;
            double newR = r + growingLine.BaseSphereRadius * 0.2; // 20% of base per tick

            if (newR > maxSphereRadius)
                newR = maxSphereRadius;

            growingLine.SphereVisual.Radius = newR;

        }

        public void StartHighlightSphere(LineMarker line)
        {
            if (line?.SphereVisual == null) return;

            EnsureSphereTimer();

            growingLine = line;
            // reset radius to base before growth
            line.SphereVisual.Radius = line.BaseSphereRadius;
            line.SphereVisual.Visible = true;
            sphereGrowTimer.Start();
        }

        public void StopHighlightSphere(LineMarker line)
        {
            if (sphereGrowTimer != null)
                sphereGrowTimer.Stop();

            if (line?.SphereVisual != null)
            {
                line.SphereVisual.Visible = false;
                // restore original size
                line.SphereVisual.Radius = line.BaseSphereRadius;
            }

            growingLine = null;
        }
        public LineManager(HelixViewport3D viewport)
        {
            this.viewport = viewport;
            lines = new List<LineMarker>();
        }


        public void SelectLine(LineMarker line)
        {
            DeselectLine();
            selectedLine = line;
            selectedLine?.Select();
        }
        public void DeselectLine()
        {
            selectedLine?.Deselect();
            selectedLine = null;
        }



        // Load lines from file (JSON). Existing lines are cleared.
        public void LoadLines(string path)
        {
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<List<LineData>>(json);
            if (data == null) return;

            // Remove existing visuals that were created by this manager
            foreach (var l in lines)
            {
                viewport.Children.Remove(l.ModelVisual);
            }
            lines.Clear();
            DeselectLine();

            // Create and add visuals
            foreach (var d in data)
            {
                var lm = LineMarker.FromData(d);
                lines.Add(lm);
                viewport.Children.Add(lm.ModelVisual);
            }
            nextId = lines.Count > 0 ? lines.Max(l => l.Id) + 1 : 1;
        }

        public LineMarker? GetLineByModel(Model3D model)
        {
            LineMarker? lineMarker = null;
            lines.ForEach(line =>
            {
                if (line.LineVisual.Content == model)
                {
                    lineMarker = line;
                }
            });
            return lineMarker;
        }
    }
}
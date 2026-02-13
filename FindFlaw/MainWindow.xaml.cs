using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using FindFlaw.Managers;
using FindFlaw.Models;
using Microsoft.Win32;
using Color = System.Windows.Media.Color;
using Path = System.IO.Path;
using Point = System.Windows.Point;

namespace FindFlaw
{
    public partial class MainWindow : Window
    {
        private ModelManager modelManager;
        private LineManager lineManager;
        private ViewportInteraction viewportInteraction;
        private UIStateManager uiStateManager;
        private Color[] ModelColors = { Colors.Blue, Colors.Gray };
        private byte emissionAlpha = 60;
        private double moveSpeed = 0.05;

        public MainWindow()
        {
            InitializeComponent();
            InitializeManagers();
            InitializeViewportEvents();
        }

        private void InitializeManagers()
        {
            modelManager = new ModelManager(viewport3D, modelRoot);
            lineManager = new LineManager(viewport3D);
            viewportInteraction = new ViewportInteraction(viewport3D);
            uiStateManager = new UIStateManager(statusLabel, linesCountLabel, selectedLineIdLabel, lineLabelTextBox);

            emissionAlpha = (byte)EmissionAlphaSlider.Value;
            EmissionAlphaLabel.Text = emissionAlpha.ToString();
        }

        private void InitializeViewportEvents()
        {
            viewport3D.MouseDown += Viewport3D_MouseDown;
            viewport3D.MouseMove += Viewport3D_MouseMove;
            viewport3D.MouseUp += Viewport3D_MouseUp;
        }

        private void ViewUpButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.PanCamera(0, +moveSpeed);
        }

        private void ViewDownButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.PanCamera(0, -moveSpeed);
        }

        private void ViewLeftButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.PanCamera(+moveSpeed, 0);
        }

        private void ViewRightButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.PanCamera(-moveSpeed, 0);
        }

        private void ViewCenterButton_Click(object sender, RoutedEventArgs e)
        {
            viewport3D.ZoomExtents();
        }

        private void ViewZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.ChangeFov(-0.5); // smaller FOV → zoom in
        }

        private void ViewZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            viewportInteraction.ChangeFov(+0.5); // larger FOV → zoom out
        }

        private void RefreshLineList()
        {
            if (lineListBox == null || lineManager == null) return;

            lineListBox.ItemsSource = null;
            lineListBox.ItemsSource = lineManager.Lines;
        }

        private void LineListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lineListBox.SelectedItem is not LineMarker marker)
                return;

            viewportInteraction.FocusCameraOnLineAnimated(marker);
        }

        private void LineListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lineListBox.SelectedItem is LineMarker line)
            {
                lineManager.SelectLine(line);
                uiStateManager.UpdateSelectedLine(line);
            }
            else
            {
                lineManager.SelectLine(null);
                uiStateManager.UpdateSelectedLine(null);
            }
        }

        private void EmissionAlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (modelManager == null) return; // Guard clause

            emissionAlpha = (byte)e.NewValue;
            if (EmissionAlphaLabel != null)
                EmissionAlphaLabel.Text = emissionAlpha.ToString();

            modelManager.SetEmissionAlpha(emissionAlpha);
        }

        private void ApplyInitialView()
        {
            if (initialViewCombo == null || modelRoot?.Content == null)
                return;

            bool topDown = initialViewCombo.SelectedIndex == 0; // 0 = Top-Down, 1 = Bottom-Up
            SetInitialView(topDown);
        }

        private void InitialViewCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If a model is already loaded, update view immediately
            if (modelRoot?.Content != null)
            {
                ApplyInitialView();
            }
        }

        private void SetInitialView(bool isTop)
        {
            if (viewport3D.Camera is not ProjectionCamera cam) return;
            if (modelRoot?.Content == null) return;

            // Get model bounds
            Rect3D bounds = modelRoot.Content.Bounds;
            if (bounds.IsEmpty) return;

            Point3D center = new Point3D(
                bounds.X + bounds.SizeX / 2,
                bounds.Y + bounds.SizeY / 2,
                bounds.Z + bounds.SizeZ / 2);

            double radius = Math.Max(bounds.SizeX, Math.Max(bounds.SizeY, bounds.SizeZ));
            if (radius < 1) radius = 1;

            double distance = radius * 2.2;   // nice view distance

            double zOffset = isTop ? +distance : -distance;

            Point3D newPos = new Point3D(center.X, center.Y, center.Z + zOffset);
            Vector3D look = new Vector3D(0, 0, isTop ? -distance : +distance);
            Vector3D up = new Vector3D(0, 1, 0); // world up direction

            cam.Position = newPos;
            cam.LookDirection = look;
            cam.UpDirection = up;
        }


        private void LoadModelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "3D Model Files (*.obj;*.stl)|*.obj;*.stl|OBJ Files (*.obj)|*.obj|STL Files (*.stl)|*.stl|All Files (*.*)|*.*",
                Title = "Open 3D Model"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    modelManager.LoadModel(dialog.FileName);
                    modelManager.SetModelColor(ModelColors[0]);
                    filePathLabel.Text = Path.GetFileName(dialog.FileName);
                    uiStateManager.SetStatus("Model loaded successfully");

                    ApplyInitialView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    filePathLabel.Text = "Error loading file";
                }
            }
        }

        private void LoadLinesButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Load Lines"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    lineManager.LoadLines(dlg.FileName);
                    RefreshLineList();
                    uiStateManager.SetStatus("Lines loaded");
                    uiStateManager.UpdateLineCount(lineManager.Count);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading lines: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void ModelColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (modelManager == null) return; // Guard clause

            Color color = ModelColors[modelColorCombo.SelectedIndex];
            modelManager.SetModelColor(color);
        }

        private void Viewport3D_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Point mousePos = e.GetPosition(viewport3D.Viewport);

            lineManager.DeselectLine();

            (bool modelHit, Point3D? hitPoint, LineMarker? line) = viewportInteraction.Get3DPointInLines(mousePos, modelManager, lineManager);
            if (!modelHit)
            {
                lineManager.SelectLine(null);
                uiStateManager.SetStatus($"No line selected.");
                uiStateManager.UpdateSelectedLine(null);
            }
            else
            {
                int id = line.Id;
                lineManager.SelectLine(line);
                uiStateManager.SetStatus($"Line selected.");
                uiStateManager.UpdateSelectedLine(line);
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (ctrl)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        viewportInteraction.MovePosition(-1, 0, 0);
                        e.Handled = true;
                        break;

                    case Key.Right:
                        viewportInteraction.MovePosition(+1, 0, 0);
                        e.Handled = true;

                        break;

                    case Key.Up:
                        viewportInteraction.MovePosition(0, +1, 0);
                        e.Handled = true;
                        break;

                    case Key.Down:
                        viewportInteraction.MovePosition(0, -1, 0);
                        e.Handled = true;
                        break;

                    case Key.OemPlus:
                    case Key.Add:
                        viewportInteraction.MovePosition(0, 0, +1);
                        e.Handled = true;
                        break;

                    case Key.OemMinus:
                    case Key.Subtract:
                        viewportInteraction.MovePosition(0, 0, -1);
                        e.Handled = true;
                        break;

                    case Key.PageUp:
                        viewportInteraction.ChangeFov(-0.5);
                        e.Handled = true;
                        break;

                    case Key.PageDown:
                        viewportInteraction.ChangeFov(+0.5);
                        e.Handled = true;
                        break;
                }
            }
            if (shift)
            {
                e.Handled = false;

                switch (e.Key)
                {
                    case Key.Left:
                        viewportInteraction.MoveTarget(-1, 0, 0);
                        e.Handled = true;
                        break;

                    case Key.Right:
                        viewportInteraction.MoveTarget(+1, 0, 0);
                        e.Handled = true;
                        break;

                    case Key.Up:
                        viewportInteraction.MoveTarget(0, +1, 0);
                        e.Handled = true;
                        break;

                    case Key.Down:
                        viewportInteraction.MoveTarget(0, -1, 0);
                        e.Handled = true;
                        break;

                    case Key.OemPlus:
                    case Key.Add:
                        viewportInteraction.MoveTarget(0, 0, +1);
                        e.Handled = true;
                        break;

                    case Key.OemMinus:
                    case Key.Subtract:
                        viewportInteraction.MoveTarget(0, 0, -1);
                        e.Handled = true;
                        break;

                    case Key.PageUp:
                        viewportInteraction.ChangeFov(-0.5);
                        e.Handled = true;
                        break;

                    case Key.PageDown:
                        viewportInteraction.ChangeFov(+0.5);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void LineColorButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LineMarker line)
            {
                lineManager.StartHighlightSphere(line);
                e.Handled = true;
            }
        }

        private void LineColorButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LineMarker line)
            {
                lineManager.StopHighlightSphere(line);
                e.Handled = true;
            }
        }
        private void LineColorButton_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is Button btn && btn.Tag is LineMarker line)
            {
                lineManager.StopHighlightSphere(line);
            }
        }

        private void Viewport3D_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void Viewport3D_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void Viewport_CameraChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
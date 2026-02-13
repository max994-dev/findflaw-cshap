using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace FindFlaw.Managers
{
    public class ModelManager
    {
        private HelixViewport3D viewport;

        private ModelVisual3D modelVisual;

        private Color color = Colors.Blue;

        private byte emissionAlpha = 20;

        private byte baseAlpha = 255;

        public ModelVisual3D ModelVisual => modelVisual;

        public ModelManager(HelixViewport3D viewport, ModelVisual3D model)
        {
            this.viewport = viewport;
            this.modelVisual = model;
        }

        public void LoadModel(string filePath)
        {
            if (modelVisual != null)
            {
                viewport.Children.Remove(modelVisual);
            }

            ModelImporter importer = new ModelImporter();
            Model3D? model = importer.Load(filePath);
            ApplyColor(model);

            modelVisual.Content = model;
            viewport.Children.Add(modelVisual);
            viewport.ZoomExtents();
        }

        public void SetBaseAlpha(byte alpha)
        {
            baseAlpha = alpha;
            ApplyColor(modelVisual.Content);
        }

        public void SetEmissionAlpha(byte emissionAlpha)
        {
            this.emissionAlpha = emissionAlpha;
            ApplyColor(modelVisual.Content);
        }

        public void SetModelColor(Color color)
        {
            this.color = color;
            ApplyColor(modelVisual.Content);
        }


        private void ApplyColor(Model3D model)
        {
            if (model is Model3DGroup groupModel)
            {
                foreach (Model3D child in groupModel.Children)
                {
                    ApplyColor(child);
                }
            }
            else if (model is GeometryModel3D geometry)
            {
                MaterialGroup mg = new MaterialGroup();
                mg.Children.Add(new DiffuseMaterial(new SolidColorBrush(color.ChangeAlpha(baseAlpha))));
                mg.Children.Add(new EmissiveMaterial(new SolidColorBrush(color.ChangeAlpha(emissionAlpha))));

                geometry.Material = mg;
                geometry.BackMaterial = mg;
            }
        }


        public bool ContainsModel(Model3D model)
        {
            if (modelVisual.Content is Model3DGroup groupModel)
            {
                foreach (Model3D child in groupModel.Children)
                {
                    if (child.Equals(model)) return true;
                }
                return false;
            }
            else if (modelVisual.Content is GeometryModel3D geometry)
            {
                return geometry.Equals(model);
            }
            return false;
        }
    }
}
﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Interfaces;

namespace Asteroids.WinForms.Classes
{
    internal sealed class GraphicPictureBox : PictureBox, IGraphicContainer
    {
        private IDictionary<System.Drawing.Color, Pen> _colorCache;
        private IEnumerable<IGraphicLine> _lastLines = new List<IGraphicLine>();
        private IEnumerable<IGraphicPolygon> _lastPolygons = new List<IGraphicPolygon>();

        public GraphicPictureBox()
        {
            _colorCache = new Dictionary<System.Drawing.Color, Pen>();
            _lastLines = new List<IGraphicLine>();
            _lastPolygons = new List<IGraphicPolygon>();
        }

        public Task Initialize(IDictionary<System.Drawing.Color, string> drawColorMap)
        {
            _colorCache = new ReadOnlyDictionary<System.Drawing.Color, Pen>(
                drawColorMap.ToDictionary(
                    kvp => kvp.Key
                    , kvp => new Pen(ColorTranslator.FromHtml(kvp.Value))
                )
            );

            Paint += OnPaint;
            return Task.CompletedTask;
        }

        public Task Draw(IEnumerable<IGraphicLine> lines, IEnumerable<IGraphicPolygon> polygons)
        {
            Invalidate();
            _lastLines = lines;
            _lastPolygons = polygons;
            return Task.CompletedTask;

        }

        private void OnPaint(object? _, PaintEventArgs e)
        {
            foreach (var line in _lastLines)
                e.Graphics.DrawLine(_colorCache[line.Color], line.Point1, line.Point2);

            foreach (var poly in _lastPolygons)
                e.Graphics.DrawPolygon(_colorCache[poly.Color], poly.Points.ToArray());
        }
    }
}
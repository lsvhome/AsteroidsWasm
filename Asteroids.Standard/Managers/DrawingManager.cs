﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Asteroids.Standard.Components;
using Asteroids.Standard.Enums;
using Asteroids.Standard.Screen;

namespace Asteroids.Standard.Managers
{
    /// <summary>
    /// Manages and optimizes the drawing of the state of <see cref="ScreenObjectBase"/>s 
    /// stored in a <see cref="CacheManager"/> to a <see cref="ScreenCanvas"/>.
    /// </summary>
    internal sealed class DrawingManager
    {
        private readonly CacheManager _cache;
        private readonly ScreenCanvas _canvas;
        private readonly TextManager _textDraw;

        /// <summary>
        /// Creates a new instance of <see cref="DrawingManager"/>
        /// </summary>
        /// <param name="cache">Screen object cache to draw.</param>
        /// <param name="canvas">Canvas to draw cache to.</param>
        public DrawingManager(CacheManager cache, ScreenCanvas canvas, TextManager textDraw)
        {
            _textDraw = textDraw;
            _cache = cache;
            _canvas = canvas;
        }

        #region Drawing Primatives

        private void DrawPolygon(IList<Point>? points)
        {
            DrawPolygon(points, Color.White);
        }

        private void DrawPolygon(IList<Point>? points, Color color)
        {
            if (points == null || !points.Any())
                return;

            _canvas.LoadPolygon(points, color);
        }

        private void DrawVector(Point origin, int offsetX, int offsetY, Color color)
        {
            _canvas.LoadVector(origin, offsetX, offsetY, color);
        }

        private void DrawDot(Point origin, Color color)
        {
            _canvas.LoadVector(origin, 0, 0, color);
        }

        #endregion

        #region Draw Objects

        /// <summary>
        /// Draw all objects to the canvas.
        /// </summary>
        public void DrawObjects()
        {
            foreach (var obj in _cache.GetIDrawableObjects())
            {
                DrawIDrawableObjects(obj);
            }
        }

        private void DrawIDrawableObjects(IDrawableObject obj)
        {
            try
            {

                foreach (var polygon in obj.Poligons)
                {
                    DrawPolygon(polygon.Points.Select(p => new Point((int)p.X, (int)p.Y)).ToList(), polygon.Color);
                }

                foreach (var vectorD in obj.Vectors)
                {
                    DrawVector(new Point((int)vectorD.Start.X, (int)vectorD.Start.Y), (int)(vectorD.End.X - vectorD.Start.X), (int)(vectorD.End.Y - vectorD.Start.Y), vectorD.Color);
                }

                foreach (var pointD in obj.Dots)
                {
                    DrawDot(new Point((int)pointD.X, (int)pointD.Y), pointD.Color);
                }


                foreach (var txt in obj.Texts)
                {
                    _textDraw.DrawText(
                        txt.TextVal
                        , txt.Justification
                        , (int)txt.Start.Y
                        , txt.LetterWidth, txt.LetterHeight
                    );
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion
    }
}
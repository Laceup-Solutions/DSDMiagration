using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace LaceupMigration.Controls
{
    public class SignatureDrawable : IDrawable
    {
        private List<PointF> _points = new();
        private List<List<PointF>> _strokes = new();
        private List<PointF> _currentStroke = new();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeColor = Colors.Black;
            canvas.StrokeSize = 2;

            // Draw all strokes
            foreach (var stroke in _strokes)
            {
                if (stroke.Count > 1)
                {
                    var path = new PathF();
                    path.MoveTo(stroke[0]);
                    for (int i = 1; i < stroke.Count; i++)
                    {
                        path.LineTo(stroke[i]);
                    }
                    canvas.DrawPath(path);
                }
                else if (stroke.Count == 1)
                {
                    canvas.FillColor = Colors.Black;
                    canvas.FillCircle(stroke[0], 2);
                }
            }

            // Draw current stroke
            if (_currentStroke.Count > 1)
            {
                var path = new PathF();
                path.MoveTo(_currentStroke[0]);
                for (int i = 1; i < _currentStroke.Count; i++)
                {
                    path.LineTo(_currentStroke[i]);
                }
                canvas.DrawPath(path);
            }
            else if (_currentStroke.Count == 1)
            {
                canvas.FillColor = Colors.Black;
                canvas.FillCircle(_currentStroke[0], 2);
            }
        }

        public void AddPoint(PointF point)
        {
            _currentStroke.Add(point);
            _points.Add(point);
        }

        public void EndStroke()
        {
            if (_currentStroke.Count > 0)
            {
                _strokes.Add(new List<PointF>(_currentStroke));
                _currentStroke.Clear();
            }
        }

        public void Clear()
        {
            _points.Clear();
            _strokes.Clear();
            _currentStroke.Clear();
        }

        public List<PointF> GetPoints()
        {
            var allPoints = new List<PointF>();
            foreach (var stroke in _strokes)
            {
                allPoints.AddRange(stroke);
                allPoints.Add(PointF.Zero); // Empty point to separate strokes
            }
            allPoints.AddRange(_currentStroke);
            return allPoints;
        }

        public void LoadPoints(List<PointF> points)
        {
            Clear();
            _strokes.Clear();
            _currentStroke.Clear();

            List<PointF> currentStroke = new();
            foreach (var point in points)
            {
                if (point == PointF.Zero || (point.X == 0 && point.Y == 0))
                {
                    // End of stroke
                    if (currentStroke.Count > 0)
                    {
                        _strokes.Add(new List<PointF>(currentStroke));
                        currentStroke.Clear();
                    }
                }
                else
                {
                    currentStroke.Add(point);
                    _points.Add(point);
                }
            }

            // Add last stroke if any
            if (currentStroke.Count > 0)
            {
                _strokes.Add(currentStroke);
            }
        }
    }
}


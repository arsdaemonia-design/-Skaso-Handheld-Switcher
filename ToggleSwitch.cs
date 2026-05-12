using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SkasoHandheldSwitcher
{
    public class ToggleSwitch : Control
    {
        private bool _checked;

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    Invalidate();
                    OnCheckedChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler CheckedChanged;

        protected virtual void OnCheckedChanged(EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }

        private const int TrackWidth = 64;
        private const int TrackHeight = 30;
        private const int ThumbSize = 24;
        private const int TrackPadding = 3;

        private Color TrackOnColor = Color.FromArgb(67, 181, 129);
        private Color TrackOffColor = Color.FromArgb(79, 84, 92);
        private Color ThumbColor = Color.White;
        private Color TextOnColor = Color.FromArgb(67, 181, 129);
        private Color TextOffColor = Color.FromArgb(114, 118, 125);

        public ToggleSwitch()
        {
            Width = 280;
            Height = 85;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cx = Width / 2;

            // Track
            float trackX = cx - TrackWidth / 2f;
            float trackY = 8;
            var trackRect = new RectangleF(trackX, trackY, TrackWidth, TrackHeight);

            Color trackColor = _checked ? TrackOnColor : TrackOffColor;
            using (var path = CreateRoundRect(trackRect, TrackHeight / 2f))
            using (var brush = new SolidBrush(trackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Thumb
            float thumbY = trackY + TrackPadding;
            float thumbX = _checked
                ? trackX + TrackWidth - ThumbSize - TrackPadding
                : trackX + TrackPadding;
            var thumbRect = new RectangleF(thumbX, thumbY, ThumbSize, ThumbSize);

            using (var brush = new SolidBrush(ThumbColor))
            {
                e.Graphics.FillEllipse(brush, thumbRect);
            }

            using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                e.Graphics.DrawEllipse(pen, thumbRect);
            }

            // Text
            string text = _checked ? "XBOX MODE ON" : "XBOX MODE OFF";
            Color textColor = _checked ? TextOnColor : TextOffColor;

            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            using (var brush = new SolidBrush(textColor))
            {
                SizeF textSize = e.Graphics.MeasureString(text, font);
                float textX = cx - textSize.Width / 2f;
                float textY = trackY + TrackHeight + 16;

                // Add padding to prevent clipping
                float padding = 5;
                if (textX < padding) textX = padding;
                if (textX + textSize.Width > Width - padding)
                    textX = Width - padding - textSize.Width;

                e.Graphics.DrawString(text, font, brush, textX, textY);
            }
        }

        private GraphicsPath CreateRoundRect(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float r2 = radius * 2;
            path.AddArc(rect.X, rect.Y, r2, r2, 180, 90);
            path.AddArc(rect.Right - r2, rect.Y, r2, r2, 270, 90);
            path.AddArc(rect.Right - r2, rect.Bottom - r2, r2, r2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r2, r2, r2, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                Checked = !Checked;
            }
        }
    }
}
﻿using System.Windows.Media;
using WpfMath.Rendering;

namespace WpfMath
{
    // Box representing other box with delimeter and script box over or under it.
    internal class OverUnderBox : Box
    {
        public OverUnderBox(Box baseBox, Box delimeterBox, Box scriptBox, double kern, bool over)
            : base()
        {
            this.BaseBox = baseBox;
            this.DelimeterBox = delimeterBox;
            this.ScriptBox = scriptBox;
            this.Kern = kern;
            this.Over = over;

            // Calculate dimensions of box.
            this.Width = baseBox.Width;
            this.Height = baseBox.Height + (over ? delimeterBox.Width : 0) +
                (over && scriptBox != null ? scriptBox.Height + scriptBox.Depth + kern : 0);
            this.Depth = baseBox.Depth + (over ? 0 : delimeterBox.Width) +
                (!over && scriptBox == null ? 0 : scriptBox.Height + scriptBox.Depth + kern);
        }

        public Box BaseBox
        {
            get;
            private set;
        }

        public Box DelimeterBox
        {
            get;
            private set;
        }

        public Box ScriptBox
        {
            get;
            private set;
        }

        // Kern between delimeter and Script.
        public double Kern
        {
            get;
            private set;
        }

        // True to draw delimeter and script over base; false to draw under base.
        public bool Over
        {
            get;
            private set;
        }

        public override void RenderGeometry(GeometryGroup geometry, double scale, double x, double y)
        {
            GeometryGroup group = new GeometryGroup();
            if (this.Over)
            {
                // Draw delimeter and script boxes over base box.
                var centerY = y - this.BaseBox.Height - this.DelimeterBox.Width;
                var translationX = x + this.DelimeterBox.Width / 2;
                var translationY = centerY + this.DelimeterBox.Width / 2;

                group.Transform.Value.Translate(translationX * scale, translationY * scale);
                group.Transform.Value.Rotate(90);

                this.DelimeterBox.RenderGeometry(group, scale, -this.DelimeterBox.Width / 2,
                    -this.DelimeterBox.Depth + this.DelimeterBox.Width / 2);

                // Draw script box as superscript.
                if (this.ScriptBox != null)
                    this.ScriptBox.RenderGeometry(geometry, scale, x, centerY - this.Kern - this.ScriptBox.Depth);
            }
            else
            {
                // Draw delimeter and script boxes under base box.
                var centerY = y + this.BaseBox.Depth + this.DelimeterBox.Width;
                var translationX = x + this.DelimeterBox.Width / 2;
                var translationY = centerY - this.DelimeterBox.Width / 2;

                group.Transform.Value.Translate(translationX * scale, translationY * scale);
                group.Transform.Value.Rotate(90);
                this.DelimeterBox.RenderGeometry(group, scale, -this.DelimeterBox.Width / 2,
                    -this.DelimeterBox.Depth + this.DelimeterBox.Width / 2);

                // Draw script box as subscript.
                if (this.ScriptBox != null)
                    this.ScriptBox.RenderGeometry(geometry, scale, x, centerY + this.Kern + this.ScriptBox.Height);
            }
        }

        public override void RenderTo(IElementRenderer renderer, double x, double y)
        {
            renderer.RenderElement(BaseBox, x, y);

            if (Over)
            {
                // Draw delimeter and script boxes over base box.
                var centerY = y - BaseBox.Height - DelimeterBox.Width;
                var translationX = x + DelimeterBox.Width / 2;
                var translationY = centerY + DelimeterBox.Width / 2;

                var transforms = new Transform[]
                {
                    new TranslateTransform(translationX, translationY),
                    new RotateTransform(90)
                };

                renderer.RenderTransformed(
                    DelimeterBox,
                    transforms,
                    -DelimeterBox.Width / 2,
                    -DelimeterBox.Depth + DelimeterBox.Width / 2);

                // Draw script box as superscript.
                if (ScriptBox != null)
                {
                    renderer.RenderElement(ScriptBox, x, centerY - Kern - ScriptBox.Depth);
                }
            }
            else
            {
                // TODO[F]: It seems like this could be generalized with the block above. The only differences are
                // translationY and the ScriptBox's Y position?

                // Draw delimeter and script boxes under base box.
                var centerY = y + BaseBox.Depth + DelimeterBox.Width;
                var translationX = x + DelimeterBox.Width / 2;
                var translationY = centerY - DelimeterBox.Width / 2;

                var transforms = new Transform[]
                {
                    new TranslateTransform(translationX, translationY),
                    new RotateTransform(90)
                };

                renderer.RenderTransformed(
                    DelimeterBox,
                    transforms,
                    -DelimeterBox.Width / 2,
                    -DelimeterBox.Depth + DelimeterBox.Width / 2);

                // Draw script box as subscript.
                if (ScriptBox != null)
                {
                    renderer.RenderElement(ScriptBox, x, centerY + Kern + ScriptBox.Height);
                }
            }
        }

        public override int GetLastFontId()
        {
            return TexFontUtilities.NoFontId;
        }
    }
}

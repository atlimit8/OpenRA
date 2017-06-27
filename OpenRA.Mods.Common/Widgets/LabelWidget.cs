#region Copyright & License Information
/*
 * Copyright 2007-2017 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Drawing;
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public enum TextAlign { Left, Center, Right }
	public enum TextVAlign { Top, Middle, Bottom }

	public class LabelWidgetInfo : WidgetInfo
	{
		[Translate] public readonly string Text = null;
		public readonly TextAlign Align = TextAlign.Left;
		public readonly TextVAlign VAlign = TextVAlign.Middle;
		public readonly string Font = ChromeMetrics.Get<string>("TextFont");
		public readonly Color TextColor = ChromeMetrics.Get<Color>("TextColor");
		public readonly bool Contrast = ChromeMetrics.Get<bool>("TextContrast");
		public readonly bool Shadow = ChromeMetrics.Get<bool>("TextShadow");
		public readonly Color ContrastColorDark = ChromeMetrics.Get<Color>("TextContrastColorDark");
		public readonly Color ContrastColorLight = ChromeMetrics.Get<Color>("TextContrastColorLight");
		public readonly bool WordWrap = false;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new LabelWidget(this, args, parent);
		}
	}

	public class LabelWidget : Widget
	{
		public new LabelWidgetInfo Info { get { return (LabelWidgetInfo)WidgetInfo; } }

		public string Text;
		public bool WordWrap = false;
		public Func<string> GetText;
		public Func<Color> GetColor;
		public Func<Color> GetContrastColorDark;
		public Func<Color> GetContrastColorLight;

		public LabelWidget(LabelWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			Text = info.Text;
			GetText = () => Text;
			GetColor = () => info.TextColor;
			GetContrastColorDark = () => info.ContrastColorDark;
			GetContrastColorLight = () => info.ContrastColorLight;
		}

		protected LabelWidget(LabelWidget other)
			: base(other)
		{
			Text = other.Text;
			WordWrap = other.WordWrap;
			GetText = other.GetText;
			GetColor = other.GetColor;
			GetContrastColorDark = other.GetContrastColorDark;
			GetContrastColorLight = other.GetContrastColorLight;
		}

		public override void Draw()
		{
			SpriteFont font;
			if (!Game.Renderer.Fonts.TryGetValue(Info.Font, out font))
				throw new ArgumentException("Requested font '{0}' was not found.".F(Info.Font));

			var text = GetText();
			if (text == null)
				return;

			var textSize = font.Measure(text);
			var position = RenderOrigin;

			if (Info.VAlign == TextVAlign.Middle)
				position += new int2(0, (Bounds.Height - textSize.Y) / 2);

			if (Info.VAlign == TextVAlign.Bottom)
				position += new int2(0, Bounds.Height - textSize.Y);

			if (Info.Align == TextAlign.Center)
				position += new int2((Bounds.Width - textSize.X) / 2, 0);

			if (Info.Align == TextAlign.Right)
				position += new int2(Bounds.Width - textSize.X, 0);

			if (WordWrap)
				text = WidgetUtils.WrapText(text, Bounds.Width, font);

			var color = GetColor();
			var bgDark = GetContrastColorDark();
			var bgLight = GetContrastColorLight();
			if (Info.Contrast)
				font.DrawTextWithContrast(text, position, color, bgDark, bgLight, 2);
			else if (Info.Shadow)
				font.DrawTextWithShadow(text, position, color, bgDark, bgLight, 1);
			else
				font.DrawText(text, position, color);
		}

		public override Widget Clone() { return new LabelWidget(this); }
	}
}

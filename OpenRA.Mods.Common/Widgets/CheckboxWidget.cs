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
	public class CheckboxWidgetInfo : ButtonWidgetInfo
	{
		public readonly string CheckType = "checked";
		public readonly Func<string> GetCheckType;
		public readonly Func<bool> IsChecked = () => false;
		public readonly int CheckOffset = 2;
		public readonly bool HasPressedState = ChromeMetrics.Get<bool>("CheckboxPressedState");

		public CheckboxWidgetInfo() { GetCheckType = () => CheckType; }

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new CheckboxWidget(this, args, parent);
		}
	}

	public class CheckboxWidget : ButtonWidget
	{
		public new CheckboxWidgetInfo Info { get { return (CheckboxWidgetInfo)WidgetInfo; } }
		public Func<string> GetCheckType;
		public Func<bool> IsChecked;

		public CheckboxWidget(CheckboxWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			GetCheckType = info.GetCheckType;
			IsChecked = info.IsChecked;
		}

		protected CheckboxWidget(CheckboxWidget other)
			: base(other)
		{
			GetCheckType = other.GetCheckType;
			IsChecked = other.IsChecked;
		}

		public override void Draw()
		{
			var disabled = IsDisabled();
			var highlighted = IsHighlighted();
			var font = Game.Renderer.Fonts[Info.Font];
			var color = GetColor();
			var colordisabled = GetColorDisabled();
			var bgDark = GetContrastColorDark();
			var bgLight = GetContrastColorLight();
			var rect = RenderBounds;
			var text = GetText();
			var textSize = font.Measure(text);
			var check = new Rectangle(rect.Location, new Size(Bounds.Height, Bounds.Height));
			var state = disabled ? "checkbox-disabled" :
						highlighted ? "checkbox-highlighted" :
						Depressed && Info.HasPressedState ? "checkbox-pressed" :
						Ui.MouseOverWidget == this ? "checkbox-hover" :
						"checkbox";

			WidgetUtils.DrawPanel(state, check);
			var position = new float2(rect.Left + rect.Height * 1.5f, RenderOrigin.Y - Info.BaseLine + (Bounds.Height - textSize.Y) / 2);

			if (Info.Contrast)
				font.DrawTextWithContrast(text, position,
					disabled ? colordisabled : color, bgDark, bgLight, 2);
			else
				font.DrawText(text, position,
					disabled ? colordisabled : color);

			if (IsChecked() || (Depressed && Info.HasPressedState && !disabled))
			{
				var checkType = GetCheckType();
				if (Info.HasPressedState && (Depressed || disabled))
					checkType += "-disabled";

				var offset = new float2(rect.Left + Info.CheckOffset, rect.Top + Info.CheckOffset);
				WidgetUtils.DrawRGBA(ChromeProvider.GetImage("checkbox-bits", checkType), offset);
			}
		}

		public override Widget Clone() { return new CheckboxWidget(this); }
	}
}

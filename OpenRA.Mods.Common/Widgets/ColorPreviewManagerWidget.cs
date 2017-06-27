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
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ColorPreviewManagerWidgetInfo : WidgetInfo
	{
		public readonly string PaletteName = "colorpicker";
		public readonly int[] RemapIndices = ChromeMetrics.Get<int[]>("ColorPickerRemapIndices");
		public readonly float Ramp = 0.05f;
		public readonly HSLColor Color;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new ColorPreviewManagerWidget(this, args, parent);
		}
	}

	public class ColorPreviewManagerWidget : Widget
	{
		public HSLColor Color;
		public new ColorPreviewManagerWidgetInfo Info { get { return (ColorPreviewManagerWidgetInfo)WidgetInfo; } }

		HSLColor cachedColor;
		readonly WorldRenderer worldRenderer;
		readonly IPalette preview;

		public ColorPreviewManagerWidget(ColorPreviewManagerWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			Color = info.Color;
			worldRenderer = args.Get<WorldRenderer>("worldRenderer");
			preview = worldRenderer.Palette(info.PaletteName).Palette;
		}

		public override void Tick()
		{
			if (cachedColor == Color)
				return;
			cachedColor = Color;

			var newPalette = new MutablePalette(preview);
			newPalette.ApplyRemap(new PlayerColorRemap(Info.RemapIndices, Color, Info.Ramp));
			worldRenderer.ReplacePalette(Info.PaletteName, newPalette);
		}
	}
}

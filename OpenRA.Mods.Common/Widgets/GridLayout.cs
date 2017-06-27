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
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class GridLayout : ILayout
	{
		ScrollPanelWidget widget;
		int2 pos;

		public GridLayout(ScrollPanelWidget w) { widget = w; }

		public void AdjustChild(Widget w)
		{
			if (widget.Children.Count == 0)
			{
				widget.ContentHeight = 2 * widget.Info.TopBottomSpacing;
				pos = new int2(widget.Info.ItemSpacing, widget.Info.TopBottomSpacing);
			}

			if (pos.X + w.Bounds.Width + widget.Info.ItemSpacing > widget.Bounds.Width - widget.Info.ScrollbarWidth)
			{
				/* start a new row */
				pos = new int2(widget.Info.ItemSpacing, widget.ContentHeight - widget.Info.TopBottomSpacing + widget.Info.ItemSpacing);
			}

			w.Bounds.X += pos.X;
			w.Bounds.Y += pos.Y;

			pos = pos.WithX(pos.X + w.Bounds.Width + widget.Info.ItemSpacing);

			widget.ContentHeight = Math.Max(widget.ContentHeight, pos.Y + w.Bounds.Height + widget.Info.TopBottomSpacing);
		}

		public void AdjustChildren() { }
	}
}
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
	public class ScrollItemWidgetInfo : ButtonWidgetInfo
	{
		public readonly string ItemKey;
		public readonly string BaseName = "scrollitem";

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new ScrollItemWidget(this, args, parent);
		}
	}

	public class ScrollItemWidget : ButtonWidget
	{
		public new ScrollItemWidgetInfo Info { get { return (ScrollItemWidgetInfo)WidgetInfo; } }
		public string ItemKey;

		public ScrollItemWidget(ScrollItemWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			IsVisible = () => false;
			VisualHeight = 0;
			IgnoreChildMouseOver = true;
			ItemKey = info.ItemKey;
		}

		public ScrollItemWidget(ScrollItemWidget other)
			: base(other)
		{
			IsVisible = () => false;
			VisualHeight = 0;
			IgnoreChildMouseOver = true;
		}

		public Func<bool> IsSelected = () => false;

		public override void Draw()
		{
			var state = IsSelected() ? Info.BaseName + "-selected" :
				Ui.MouseOverWidget == this ? Info.BaseName + "-hover" :
				null;

			if (state != null)
				WidgetUtils.DrawPanel(state, RenderBounds);
		}

		public override Widget Clone() { return new ScrollItemWidget(this); }

		public static ScrollItemWidget Setup(ScrollItemWidget template, Func<bool> isSelected, Action onClick)
		{
			var w = template.Clone() as ScrollItemWidget;
			w.IsVisible = () => true;
			w.IsSelected = isSelected;
			w.OnClick = onClick;
			return w;
		}

		public static ScrollItemWidget Setup(ScrollItemWidget template, Func<bool> isSelected, Action onClick, Action onDoubleClick)
		{
			var w = Setup(template, isSelected, onClick);
			w.OnDoubleClick = onDoubleClick;
			return w;
		}

		public static ScrollItemWidget Setup(string key, ScrollItemWidget template, Func<bool> isSelected, Action onClick, Action onDoubleClick)
		{
			var w = Setup(template, isSelected, onClick);
			w.OnDoubleClick = onDoubleClick;
			w.ItemKey = key;
			return w;
		}
	}
}

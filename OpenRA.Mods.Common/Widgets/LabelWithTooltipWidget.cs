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
	public class LabelWithTooltipWidgetInfo : LabelWidgetInfo
	{
		public readonly string TooltipTemplate;
		public readonly string TooltipContainer;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new LabelWithTooltipWidget(this, args, parent);
		}
	}

	public class LabelWithTooltipWidget : LabelWidget
	{
		public new LabelWithTooltipWidgetInfo Info { get { return (LabelWithTooltipWidgetInfo)WidgetInfo; } }

		readonly Lazy<TooltipContainerWidget> tooltipContainer;

		public Func<string> GetTooltipText = () => "";

		public LabelWithTooltipWidget(LabelWithTooltipWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(info.TooltipContainer));
		}

		protected LabelWithTooltipWidget(LabelWithTooltipWidget other)
			: base(other)
		{
			var info = other.Info;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(info.TooltipContainer));

			GetTooltipText = other.GetTooltipText;
		}

		public override Widget Clone() { return new LabelWithTooltipWidget(this); }

		public override void MouseEntered()
		{
			if (Info.TooltipContainer == null)
				return;

			if (GetTooltipText != null)
				tooltipContainer.Value.SetTooltip(Info.TooltipTemplate, new WidgetArgs() { { "getText", GetTooltipText } });
		}

		public override void MouseExited()
		{
			if (Info.TooltipContainer == null)
				return;

			tooltipContainer.Value.RemoveTooltip();
		}
	}
}

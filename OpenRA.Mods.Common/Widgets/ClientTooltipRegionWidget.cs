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
using OpenRA.Network;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ClientTooltipRegionWidgetInfo : WidgetInfo
	{
		public readonly string Template;
		public readonly string TooltipContainer;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new ClientTooltipRegionWidget(this, args, parent);
		}
	}

	public class ClientTooltipRegionWidget : Widget
	{
		public new ClientTooltipRegionWidgetInfo Info { get { return (ClientTooltipRegionWidgetInfo)WidgetInfo; } }

		readonly Lazy<TooltipContainerWidget> tooltipContainer;
		OrderManager orderManager;
		int clientIndex;

		public ClientTooltipRegionWidget(ClientTooltipRegionWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			tooltipContainer = Exts.Lazy(() => Ui.Root.Get<TooltipContainerWidget>(info.TooltipContainer));
		}

		protected ClientTooltipRegionWidget(ClientTooltipRegionWidget other)
			: base(other)
		{
			tooltipContainer = Exts.Lazy(() => Ui.Root.Get<TooltipContainerWidget>(Info.TooltipContainer));
			orderManager = other.orderManager;
			clientIndex = other.clientIndex;
		}

		public override Widget Clone() { return new ClientTooltipRegionWidget(this); }

		public void Bind(OrderManager orderManager, int clientIndex)
		{
			this.orderManager = orderManager;
			this.clientIndex = clientIndex;
		}

		public override void MouseEntered()
		{
			if (Info.TooltipContainer == null)
				return;
			tooltipContainer.Value.SetTooltip(Info.Template, new WidgetArgs() { { "orderManager", orderManager }, { "clientIndex", clientIndex } });
		}

		public override void MouseExited()
		{
			if (Info.TooltipContainer == null)
				return;
			tooltipContainer.Value.RemoveTooltip();
		}
	}
}

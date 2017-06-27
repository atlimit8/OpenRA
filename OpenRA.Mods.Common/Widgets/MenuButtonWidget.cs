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

using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class MenuButtonWidgetInfo : ButtonWidgetInfo
	{
		public readonly string MenuContainer = "INGAME_MENU";
		public readonly bool Pause = true;
		public readonly bool HideIngameUI = true;
		public readonly bool DisableWorldSounds = false;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new MenuButtonWidget(this, args, parent);
		}
	}

	public class MenuButtonWidget : ButtonWidget
	{
		public new MenuButtonWidgetInfo Info { get { return (MenuButtonWidgetInfo)WidgetInfo; } }

		public MenuButtonWidget(MenuButtonWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent) { }

		protected MenuButtonWidget(MenuButtonWidget other)
			: base(other) { }
	}
}

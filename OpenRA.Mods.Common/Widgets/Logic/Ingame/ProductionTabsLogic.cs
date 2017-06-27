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

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class ProductionTabsLogic : ChromeLogic
	{
		readonly ProductionTabsWidget tabs;
		readonly World world;

		void SetupProductionGroupButton(ProductionTypeButtonWidget button)
		{
			if (button == null)
				return;

			Action<bool> selectTab = reverse =>
			{
				if (tabs.QueueGroup == button.Info.ProductionGroup)
					tabs.SelectNextTab(reverse);
				else
					tabs.QueueGroup = button.Info.ProductionGroup;

				tabs.PickUpCompletedBuilding();
			};

			Func<ButtonWidget, Hotkey> getKey = _ => Hotkey.Invalid;
			if (!string.IsNullOrEmpty(button.Info.HotkeyName))
			{
				var ks = Game.Settings.Keys;
				var field = ks.GetType().GetField(button.Info.HotkeyName);
				if (field != null)
					getKey = _ => (Hotkey)field.GetValue(ks);
			}

			button.IsDisabled = () => tabs.Groups[button.Info.ProductionGroup].Tabs.Count == 0;
			button.OnMouseUp = mi => selectTab(mi.Modifiers.HasModifier(Modifiers.Shift));
			button.OnKeyPress = e => selectTab(e.Modifiers.HasModifier(Modifiers.Shift));
			button.IsHighlighted = () => tabs.QueueGroup == button.Info.ProductionGroup;
			button.GetKey = getKey;

			var chromeName = button.Info.ProductionGroup.ToLowerInvariant();
			var icon = button.Get<ImageWidget>("ICON");
			icon.GetImageName = () => button.IsDisabled() ? chromeName + "-disabled" :
				tabs.Groups[button.Info.ProductionGroup].Alert ? chromeName + "-alert" : chromeName;
		}

		[ObjectCreator.UseCtor]
		public ProductionTabsLogic(Widget widget, World world)
		{
			this.world = world;
			tabs = widget.Get<ProductionTabsWidget>("PRODUCTION_TABS");
			world.ActorAdded += tabs.ActorChanged;
			world.ActorRemoved += tabs.ActorChanged;
			Game.BeforeGameStart += UnregisterEvents;

			var typesContainer = Ui.Root.Get(tabs.Info.TypesContainer);
			foreach (var i in typesContainer.Children)
				SetupProductionGroupButton(i as ProductionTypeButtonWidget);

			var background = Ui.Root.GetOrNull(tabs.Info.BackgroundContainer);
			if (background != null)
			{
				var palette = tabs.Parent.Get<ProductionPaletteWidget>(tabs.Info.PaletteWidget);
				var icontemplate = background.Get("ICON_TEMPLATE");

				Action<int, int> updateBackground = (oldCount, newCount) =>
				{
					background.RemoveChildren();

					for (var i = 0; i < newCount; i++)
					{
						var x = i % palette.Info.Columns;
						var y = i / palette.Info.Columns;

						var bg = icontemplate.Clone();
						bg.Bounds.X = palette.Info.IconSize.X * x;
						bg.Bounds.Y = palette.Info.IconSize.Y * y;
						background.AddChild(bg);
					}
				};

				palette.OnIconCountChanged += updateBackground;

				// Set the initial palette state
				updateBackground(0, 0);
			}
		}

		void UnregisterEvents()
		{
			Game.BeforeGameStart -= UnregisterEvents;
			world.ActorAdded -= tabs.ActorChanged;
			world.ActorRemoved -= tabs.ActorChanged;
		}
	}
}

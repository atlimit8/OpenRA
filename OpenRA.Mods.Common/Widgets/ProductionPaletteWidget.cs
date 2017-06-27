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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Orders;
using OpenRA.Mods.Common.Traits;
using OpenRA.Mods.Common.Traits.Render;
using OpenRA.Network;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ProductionIcon
	{
		public ActorInfo Actor;
		public string Name;
		public Hotkey Hotkey;
		public Sprite Sprite;
		public PaletteReference Palette;
		public PaletteReference IconClockPalette;
		public PaletteReference IconDarkenPalette;
		public float2 Pos;
		public List<ProductionItem> Queued;
		public ProductionQueue ProductionQueue;
	}

	public enum ReadyTextStyleOptions { Solid, AlternatingColor, Blinking }

	public class ProductionPaletteWidgetInfo : WidgetInfo
	{
		public readonly ReadyTextStyleOptions ReadyTextStyle = ReadyTextStyleOptions.AlternatingColor;
		public readonly Color ReadyTextAltColor = Color.Gold;
		public readonly int Columns = 3;
		public readonly int2 IconSize = new int2(64, 48);
		public readonly int2 IconMargin = int2.Zero;
		public readonly int2 IconSpriteOffset = int2.Zero;

		public readonly string TabClick = null;
		public readonly string DisabledTabClick = null;
		public readonly string TooltipContainer;
		public readonly string TooltipTemplate = "PRODUCTION_TOOLTIP";

		public readonly string ClockAnimation = "clock";
		public readonly string ClockSequence = "idle";
		public readonly string ClockPalette = "chrome";

		public readonly string NotBuildableAnimation = "clock";
		public readonly string NotBuildableSequence = "idle";
		public readonly string NotBuildablePalette = "chrome";

		[Translate] public readonly string ReadyText = "";
		[Translate] public readonly string HoldText = "";

		public readonly int MinimumRows = 4;
		public readonly int MaximumRows = int.MaxValue;

		public readonly int IconRowOffset = 0;
		public readonly int MaxIconRowOffset = int.MaxValue;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new ProductionPaletteWidget(this, args, parent);
		}
	}

	public class ProductionPaletteWidget : Widget
	{
		public new ProductionPaletteWidgetInfo Info { get { return (ProductionPaletteWidgetInfo)WidgetInfo; } }
		public int DisplayedIconCount { get; private set; }
		public int TotalIconCount { get; private set; }
		public event Action<int, int> OnIconCountChanged = (a, b) => { };

		public ProductionIcon TooltipIcon { get; private set; }
		public Func<ProductionIcon> GetTooltipIcon;
		public readonly World World;
		readonly OrderManager orderManager;

		public int IconRowOffset = 0;
		public int MaxIconRowOffset = int.MaxValue;

		Lazy<TooltipContainerWidget> tooltipContainer;
		ProductionQueue currentQueue;

		public ProductionQueue CurrentQueue
		{
			get { return currentQueue; }
			set { currentQueue = value; RefreshIcons(); }
		}

		public override Rectangle EventBounds { get { return eventBounds; } }
		Dictionary<Rectangle, ProductionIcon> icons = new Dictionary<Rectangle, ProductionIcon>();
		Animation cantBuild, clock;
		Rectangle eventBounds = Rectangle.Empty;
		readonly WorldRenderer worldRenderer;
		SpriteFont overlayFont;
		float2 holdOffset, readyOffset, timeOffset, queuedOffset;

		public ProductionPaletteWidget(ProductionPaletteWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			orderManager = args.Get<OrderManager>("orderManager");
			World = args.Get<World>("world");
			worldRenderer = args.Get<WorldRenderer>("worldRenderer");
			GetTooltipIcon = () => TooltipIcon;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(info.TooltipContainer));

			cantBuild = new Animation(World, info.NotBuildableAnimation);
			cantBuild.PlayFetchIndex(info.NotBuildableSequence, () => 0);
			clock = new Animation(World, Info.ClockAnimation);
			MaxIconRowOffset = info.MaxIconRowOffset;
		}

		public void ScrollDown()
		{
			if (CanScrollDown)
				IconRowOffset++;
		}

		public bool CanScrollDown
		{
			get
			{
				var totalRows = (TotalIconCount + Info.Columns - 1) / Info.Columns;

				return IconRowOffset < totalRows - MaxIconRowOffset;
			}
		}

		public void ScrollUp()
		{
			if (CanScrollUp)
				IconRowOffset--;
		}

		public bool CanScrollUp
		{
			get { return IconRowOffset > 0; }
		}

		public void ScrollToTop()
		{
			IconRowOffset = 0;
		}

		public IEnumerable<ActorInfo> AllBuildables
		{
			get
			{
				if (CurrentQueue == null)
					return Enumerable.Empty<ActorInfo>();

				return CurrentQueue.AllItems().OrderBy(a => a.TraitInfo<BuildableInfo>().BuildPaletteOrder);
			}
		}

		public override void Tick()
		{
			TotalIconCount = AllBuildables.Count();

			if (CurrentQueue != null && !CurrentQueue.Actor.IsInWorld)
				CurrentQueue = null;

			if (CurrentQueue != null)
				RefreshIcons();
		}

		public override void MouseEntered()
		{
			if (Info.TooltipContainer != null)
				tooltipContainer.Value.SetTooltip(Info.TooltipTemplate,
					new WidgetArgs() { { "player", World.LocalPlayer }, { "getTooltipIcon", GetTooltipIcon } });
		}

		public override void MouseExited()
		{
			if (Info.TooltipContainer != null)
				tooltipContainer.Value.RemoveTooltip();
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			var icon = icons.Where(i => i.Key.Contains(mi.Location))
				.Select(i => i.Value).FirstOrDefault();

			if (mi.Event == MouseInputEvent.Move)
				TooltipIcon = icon;

			if (icon == null)
				return false;

			// Eat mouse-up events
			if (mi.Event != MouseInputEvent.Down)
				return true;

			return HandleEvent(icon, mi.Button, mi.Modifiers);
		}

		protected bool PickUpCompletedBuildingIcon(ProductionIcon icon, ProductionItem item)
		{
			var actor = World.Map.Rules.Actors[icon.Name];

			if (item != null && item.Done && actor.HasTraitInfo<BuildingInfo>())
			{
				World.OrderGenerator = new PlaceBuildingOrderGenerator(CurrentQueue, icon.Name);
				return true;
			}

			return false;
		}

		public void PickUpCompletedBuilding()
		{
			foreach (var icon in icons.Values)
			{
				var item = icon.Queued.FirstOrDefault();
				if (PickUpCompletedBuildingIcon(icon, item))
					break;
			}
		}

		bool HandleLeftClick(ProductionItem item, ProductionIcon icon, int handleCount)
		{
			if (PickUpCompletedBuildingIcon(icon, item))
			{
				Game.Sound.Play(SoundType.UI, Info.TabClick);
				return true;
			}

			if (item != null && item.Paused)
			{
				// Resume a paused item
				Game.Sound.Play(SoundType.UI, Info.TabClick);
				World.IssueOrder(Order.PauseProduction(CurrentQueue.Actor, icon.Name, false));
				return true;
			}

			if (CurrentQueue.BuildableItems().Any(a => a.Name == icon.Name))
			{
				// Queue a new item
				Game.Sound.Play(SoundType.UI, Info.TabClick);
				Game.Sound.PlayNotification(World.Map.Rules, World.LocalPlayer, "Speech", CurrentQueue.Info.QueuedAudio, World.LocalPlayer.Faction.InternalName);
				World.IssueOrder(Order.StartProduction(CurrentQueue.Actor, icon.Name, handleCount));
				return true;
			}

			return false;
		}

		bool HandleRightClick(ProductionItem item, ProductionIcon icon, int handleCount)
		{
			if (item == null)
				return false;

			Game.Sound.Play(SoundType.UI, Info.TabClick);

			if (item.Paused || item.Done || item.TotalCost == item.RemainingCost)
			{
				// Instant cancel of things we have not started yet and things that are finished
				Game.Sound.PlayNotification(World.Map.Rules, World.LocalPlayer, "Speech", CurrentQueue.Info.CancelledAudio, World.LocalPlayer.Faction.InternalName);
				World.IssueOrder(Order.CancelProduction(CurrentQueue.Actor, icon.Name, handleCount));
			}
			else
			{
				// Pause an existing item
				Game.Sound.PlayNotification(World.Map.Rules, World.LocalPlayer, "Speech", CurrentQueue.Info.OnHoldAudio, World.LocalPlayer.Faction.InternalName);
				World.IssueOrder(Order.PauseProduction(CurrentQueue.Actor, icon.Name, true));
			}

			return true;
		}

		bool HandleMiddleClick(ProductionItem item, ProductionIcon icon, int handleCount)
		{
			if (item == null)
				return false;

			// Directly cancel, skipping "on-hold"
			Game.Sound.Play(SoundType.UI, Info.TabClick);
			Game.Sound.PlayNotification(World.Map.Rules, World.LocalPlayer, "Speech", CurrentQueue.Info.CancelledAudio, World.LocalPlayer.Faction.InternalName);
			World.IssueOrder(Order.CancelProduction(CurrentQueue.Actor, icon.Name, handleCount));

			return true;
		}

		bool HandleEvent(ProductionIcon icon, MouseButton btn, Modifiers modifiers)
		{
			var startCount = modifiers.HasModifier(Modifiers.Shift) ? 5 : 1;
			var cancelCount = modifiers.HasModifier(Modifiers.Ctrl) ? CurrentQueue.QueueLength : startCount;
			var item = icon.Queued.FirstOrDefault();
			var handled = btn == MouseButton.Left ? HandleLeftClick(item, icon, startCount)
				: btn == MouseButton.Right ? HandleRightClick(item, icon, cancelCount)
				: btn == MouseButton.Middle ? HandleMiddleClick(item, icon, cancelCount)
				: false;

			if (!handled)
				Game.Sound.Play(SoundType.UI, Info.DisabledTabClick);

			return true;
		}

		public override bool HandleKeyPress(KeyInput e)
		{
			if (e.Event == KeyInputEvent.Up || CurrentQueue == null)
				return false;

			var hotkey = Hotkey.FromKeyInput(e);
			var batchModifiers = e.Modifiers.HasModifier(Modifiers.Shift) ? Modifiers.Shift : Modifiers.None;
			if (batchModifiers != Modifiers.None)
				hotkey = new Hotkey(hotkey.Key, hotkey.Modifiers ^ Modifiers.Shift);

			var toBuild = icons.Values.FirstOrDefault(i => i.Hotkey == hotkey);
			return toBuild != null ? HandleEvent(toBuild, MouseButton.Left, batchModifiers) : false;
		}

		public void RefreshIcons()
		{
			icons = new Dictionary<Rectangle, ProductionIcon>();
			var producer = CurrentQueue != null ? CurrentQueue.MostLikelyProducer() : default(TraitPair<Production>);
			if (CurrentQueue == null || producer.Trait == null)
			{
				if (DisplayedIconCount != 0)
				{
					OnIconCountChanged(DisplayedIconCount, 0);
					DisplayedIconCount = 0;
				}

				return;
			}

			var oldIconCount = DisplayedIconCount;
			DisplayedIconCount = 0;

			var ks = Game.Settings.Keys;
			var rb = RenderBounds;
			var faction = producer.Trait.Faction;

			foreach (var item in AllBuildables.Skip(IconRowOffset * Info.Columns).Take(MaxIconRowOffset * Info.Columns))
			{
				var x = DisplayedIconCount % Info.Columns;
				var y = DisplayedIconCount / Info.Columns;
				var rect = new Rectangle(rb.X + x * (Info.IconSize.X + Info.IconMargin.X), rb.Y + y * (Info.IconSize.Y + Info.IconMargin.Y), Info.IconSize.X, Info.IconSize.Y);

				var rsi = item.TraitInfo<RenderSpritesInfo>();
				var icon = new Animation(World, rsi.GetImage(item, World.Map.Rules.Sequences, faction));
				var bi = item.TraitInfo<BuildableInfo>();
				icon.Play(bi.Icon);

				var pi = new ProductionIcon()
				{
					Actor = item,
					Name = item.Name,
					Hotkey = ks.GetProductionHotkey(DisplayedIconCount),
					Sprite = icon.Image,
					Palette = worldRenderer.Palette(bi.IconPalette),
					IconClockPalette = worldRenderer.Palette(Info.ClockPalette),
					IconDarkenPalette = worldRenderer.Palette(Info.NotBuildablePalette),
					Pos = new float2(rect.Location),
					Queued = currentQueue.AllQueued().Where(a => a.Item == item.Name).ToList(),
					ProductionQueue = currentQueue
				};

				icons.Add(rect, pi);
				DisplayedIconCount++;
			}

			eventBounds = icons.Any() ? icons.Keys.Aggregate(Rectangle.Union) : Rectangle.Empty;

			if (oldIconCount != DisplayedIconCount)
				OnIconCountChanged(oldIconCount, DisplayedIconCount);
		}

		public override void Draw()
		{
			var iconOffset = 0.5f * Info.IconSize.ToFloat2() + Info.IconSpriteOffset;

			overlayFont = Game.Renderer.Fonts["TinyBold"];
			timeOffset = iconOffset - overlayFont.Measure(WidgetUtils.FormatTime(0, World.Timestep)) / 2;
			queuedOffset = new float2(4, 2);
			holdOffset = iconOffset - overlayFont.Measure(Info.HoldText) / 2;
			readyOffset = iconOffset - overlayFont.Measure(Info.ReadyText) / 2;

			if (CurrentQueue == null)
				return;

			var buildableItems = CurrentQueue.BuildableItems();

			var pios = currentQueue.Actor.Owner.PlayerActor.TraitsImplementing<IProductionIconOverlay>();

			// Icons
			foreach (var icon in icons.Values)
			{
				WidgetUtils.DrawSHPCentered(icon.Sprite, icon.Pos + iconOffset, icon.Palette);

				// Draw the ProductionIconOverlay's sprite
				var pio = pios.FirstOrDefault(p => p.IsOverlayActive(icon.Actor));
				if (pio != null)
					WidgetUtils.DrawSHPCentered(pio.Sprite, icon.Pos + iconOffset + pio.Offset(Info.IconSize), worldRenderer.Palette(pio.Palette), 1f);

				// Build progress
				if (icon.Queued.Count > 0)
				{
					var first = icon.Queued[0];
					clock.PlayFetchIndex(Info.ClockSequence,
						() => (first.TotalTime - first.RemainingTime)
							* (clock.CurrentSequence.Length - 1) / first.TotalTime);
					clock.Tick();

					WidgetUtils.DrawSHPCentered(clock.Image, icon.Pos + iconOffset, icon.IconClockPalette);
				}
				else if (!buildableItems.Any(a => a.Name == icon.Name))
					WidgetUtils.DrawSHPCentered(cantBuild.Image, icon.Pos + iconOffset, icon.IconDarkenPalette);
			}

			// Overlays
			foreach (var icon in icons.Values)
			{
				var total = icon.Queued.Count;
				if (total > 0)
				{
					var first = icon.Queued[0];
					var waiting = first != CurrentQueue.CurrentItem() && !first.Done;
					if (first.Done)
					{
						if (Info.ReadyTextStyle == ReadyTextStyleOptions.Solid || orderManager.LocalFrameNumber * worldRenderer.World.Timestep / 360 % 2 == 0)
							overlayFont.DrawTextWithContrast(Info.ReadyText, icon.Pos + readyOffset, Color.White, Color.Black, 1);
						else if (Info.ReadyTextStyle == ReadyTextStyleOptions.AlternatingColor)
							overlayFont.DrawTextWithContrast(Info.ReadyText, icon.Pos + readyOffset, Info.ReadyTextAltColor, Color.Black, 1);
					}
					else if (first.Paused)
						overlayFont.DrawTextWithContrast(Info.HoldText,
							icon.Pos + holdOffset,
							Color.White, Color.Black, 1);
					else if (!waiting)
						overlayFont.DrawTextWithContrast(WidgetUtils.FormatTime(first.RemainingTimeActual, World.Timestep),
							icon.Pos + timeOffset,
							Color.White, Color.Black, 1);

					if (total > 1 || waiting)
						overlayFont.DrawTextWithContrast(total.ToString(),
							icon.Pos + queuedOffset,
							Color.White, Color.Black, 1);
				}
			}
		}

		public override string GetCursor(int2 pos)
		{
			var icon = icons.Where(i => i.Key.Contains(pos))
				.Select(i => i.Value).FirstOrDefault();

			return icon != null ? base.GetCursor(pos) : null;
		}
	}
}

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
using System.Linq;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class HotkeyEntryWidgetInfo : WidgetInfo
	{
		public readonly Hotkey Key;
		public readonly int VisualHeight = 1;
		public readonly int LeftMargin = 5;
		public readonly int RightMargin = 5;
		public readonly string Font = ChromeMetrics.Get<string>("HotkeyFont");
		public readonly Color TextColor = ChromeMetrics.Get<Color>("HotkeyColor");
		public readonly Color TextColorDisabled = ChromeMetrics.Get<Color>("HotkeyColorDisabled");

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new HotkeyEntryWidget(this, args, parent);
		}
	}

	public class HotkeyEntryWidget : Widget
	{
		public new HotkeyEntryWidgetInfo Info { get { return (HotkeyEntryWidgetInfo)WidgetInfo; } }
		public Hotkey Key;
		public Action OnLoseFocus = () => { };

		public Func<bool> IsDisabled = () => false;

		public HotkeyEntryWidget(HotkeyEntryWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent) { }
		protected HotkeyEntryWidget(HotkeyEntryWidget widget)
			: base(widget) { }

		public override bool YieldKeyboardFocus()
		{
			OnLoseFocus();
			return base.YieldKeyboardFocus();
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (IsDisabled())
				return false;

			if (mi.Event != MouseInputEvent.Down)
				return false;

			// Attempt to take keyboard focus
			if (!RenderBounds.Contains(mi.Location) || !TakeKeyboardFocus())
				return false;

			blinkCycle = 15;

			return true;
		}

		static readonly Keycode[] IgnoreKeys = new Keycode[]
		{
			Keycode.RSHIFT, Keycode.LSHIFT,
			Keycode.RCTRL, Keycode.LCTRL,
			Keycode.RALT, Keycode.LALT,
			Keycode.RGUI, Keycode.LGUI
		};

		public override bool HandleKeyPress(KeyInput e)
		{
			if (IsDisabled() || e.Event == KeyInputEvent.Up)
				return false;

			if (!HasKeyboardFocus || IgnoreKeys.Contains(e.Key))
				return false;

			Key = Hotkey.FromKeyInput(e);

			YieldKeyboardFocus();

			return true;
		}

		protected int blinkCycle = 15;
		protected bool showEntry = true;

		public override void Tick()
		{
			if (HasKeyboardFocus && --blinkCycle <= 0)
			{
				blinkCycle = 15;
				showEntry ^= true;
			}
		}

		public override void Draw()
		{
			var apparentText = Key.DisplayString();

			var font = Game.Renderer.Fonts[Info.Font];
			var pos = RenderOrigin;

			var textSize = font.Measure(apparentText);

			var disabled = IsDisabled();
			var state = disabled ? "textfield-disabled" :
				HasKeyboardFocus ? "textfield-focused" :
					Ui.MouseOverWidget == this ? "textfield-hover" :
					"textfield";

			WidgetUtils.DrawPanel(state, RenderBounds);

			// Blink the current entry to indicate focus
			if (HasKeyboardFocus && !showEntry)
				return;

			// Inset text by the margin and center vertically
			var textPos = pos + new int2(Info.LeftMargin, (Bounds.Height - textSize.Y) / 2 - Info.VisualHeight);

			// Scissor when the text overflows
			if (textSize.X > Bounds.Width - Info.LeftMargin - Info.RightMargin)
			{
				Game.Renderer.EnableScissor(new Rectangle(pos.X + Info.LeftMargin, pos.Y,
					Bounds.Width - Info.LeftMargin - Info.RightMargin, Bounds.Bottom));
			}

			var color = disabled ? Info.TextColorDisabled : Info.TextColor;
			font.DrawText(apparentText, textPos, color);

			if (textSize.X > Bounds.Width - Info.LeftMargin - Info.RightMargin)
				Game.Renderer.DisableScissor();
		}

		public override Widget Clone() { return new HotkeyEntryWidget(this); }
	}
}

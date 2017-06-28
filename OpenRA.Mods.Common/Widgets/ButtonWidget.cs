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
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class ButtonWidgetInfo : WidgetInfo
	{
		public readonly string TooltipContainer;
		public readonly string TooltipTemplate = "BUTTON_TOOLTIP";

		public readonly bool DisableKeyRepeat = false;

		[Translate] public readonly string Text = "";

		public readonly string Background = "button";
		public readonly int VisualHeight = ChromeMetrics.Get<int>("ButtonDepth");
		public readonly int BaseLine = ChromeMetrics.Get<int>("ButtonBaseLine");
		public readonly string Font = ChromeMetrics.Get<string>("ButtonFont");
		public readonly Color TextColor = ChromeMetrics.Get<Color>("ButtonTextColor");
		public readonly Color TextColorDisabled = ChromeMetrics.Get<Color>("ButtonTextColorDisabled");
		public readonly bool Contrast = ChromeMetrics.Get<bool>("ButtonTextContrast");
		public readonly bool Shadow = ChromeMetrics.Get<bool>("ButtonTextShadow");
		public readonly Color ContrastColorDark = ChromeMetrics.Get<Color>("ButtonTextContrastColorDark");
		public readonly Color ContrastColorLight = ChromeMetrics.Get<Color>("ButtonTextContrastColorLight");
		public readonly bool Disabled = false;
		public readonly bool Highlighted = false;
		[Translate] public readonly string TooltipText;
		[Translate] public readonly string TooltipDesc;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new ButtonWidget(this, args, parent);
		}
	}

	public class ButtonWidget : Widget
	{
		public new ButtonWidgetInfo Info { get { return (ButtonWidgetInfo)WidgetInfo; } }
		public Func<ButtonWidget, Hotkey> GetKey = _ => Hotkey.Invalid;
		public int VisualHeight;

		public Hotkey Key
		{
			get { return GetKey(this); }
			set { GetKey = _ => value; }
		}

		public string Text = "";
		public string Background = "button";
		public bool Depressed = false;
		public bool Disabled = false;
		public Color TextColor;
		public Func<string> GetText;
		public Func<Color> GetColor;
		public Func<Color> GetColorDisabled;
		public Func<Color> GetContrastColorDark;
		public Func<Color> GetContrastColorLight;
		public Func<bool> IsDisabled;
		public Func<bool> IsHighlighted;
		public Action<MouseInput> OnMouseDown = _ => { };
		public Action<MouseInput> OnMouseUp = _ => { };

		Lazy<TooltipContainerWidget> tooltipContainer;
		public Func<string> GetTooltipText;
		public Func<string> GetTooltipDesc;

		// Equivalent to OnMouseUp, but without an input arg
		public Action OnClick = () => { };
		public Action OnDoubleClick = () => { };
		public Action<KeyInput> OnKeyPress = _ => { };

		protected readonly Ruleset ModRules;

		public ButtonWidget(ButtonWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			ModRules = args.Get<ModData>("modData").DefaultRules;
			Text = info.Text;
			TextColor = info.TextColor;
			Background = info.Background;
			Disabled = info.Disabled;
			VisualHeight = info.VisualHeight;

			GetText = () => Text;
			GetColor = () => TextColor;
			GetColorDisabled = () => info.TextColorDisabled;
			GetContrastColorDark = () => info.ContrastColorDark;
			GetContrastColorLight = () => info.ContrastColorLight;
			OnMouseUp = _ => OnClick();
			OnKeyPress = _ => OnClick();
			IsDisabled = () => Disabled;
			IsHighlighted = () => info.Highlighted;
			GetTooltipText = () => info.TooltipText;
			GetTooltipDesc = () => info.TooltipDesc;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(info.TooltipContainer));
		}

		protected ButtonWidget(ButtonWidget other)
			: base(other)
		{
			ModRules = other.ModRules;

			Text = other.Text;
			Depressed = other.Depressed;
			Background = other.Background;
			VisualHeight = other.VisualHeight;
			GetText = other.GetText;
			GetColor = other.GetColor;
			GetColorDisabled = other.GetColorDisabled;
			GetContrastColorDark = other.GetContrastColorDark;
			GetContrastColorLight = other.GetContrastColorLight;
			OnMouseDown = other.OnMouseDown;
			Disabled = other.Disabled;
			IsDisabled = other.IsDisabled;
			IsHighlighted = other.IsHighlighted;

			OnMouseUp = mi => OnClick();
			OnKeyPress = _ => OnClick();

			GetTooltipText = other.GetTooltipText;
			GetTooltipDesc = other.GetTooltipDesc;
			tooltipContainer = Exts.Lazy(() =>
				Ui.Root.Get<TooltipContainerWidget>(Info.TooltipContainer));
		}

		public override bool YieldMouseFocus(MouseInput mi)
		{
			Depressed = false;
			return base.YieldMouseFocus(mi);
		}

		public override bool HandleKeyPress(KeyInput e)
		{
			if (Hotkey.FromKeyInput(e) != Key || e.Event != KeyInputEvent.Down || (Info.DisableKeyRepeat && e.IsRepeat))
				return false;

			if (!IsDisabled())
			{
				OnKeyPress(e);
				Game.Sound.PlayNotification(ModRules, null, "Sounds", "ClickSound", null);
			}
			else
				Game.Sound.PlayNotification(ModRules, null, "Sounds", "ClickDisabledSound", null);

			return true;
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (mi.Button != MouseButton.Left)
				return false;

			if (mi.Event == MouseInputEvent.Down && !TakeMouseFocus(mi))
				return false;

			var disabled = IsDisabled();
			if (HasMouseFocus && mi.Event == MouseInputEvent.Up && mi.MultiTapCount == 2)
			{
				if (!disabled)
				{
					OnDoubleClick();
					return YieldMouseFocus(mi);
				}
			}
			else if (HasMouseFocus && mi.Event == MouseInputEvent.Up)
			{
				// Only fire the onMouseUp event if we successfully lost focus, and were pressed
				if (Depressed && !disabled)
					OnMouseUp(mi);

				return YieldMouseFocus(mi);
			}

			if (mi.Event == MouseInputEvent.Down)
			{
				// OnMouseDown returns false if the button shouldn't be pressed
				if (!disabled)
				{
					OnMouseDown(mi);
					Depressed = true;
					Game.Sound.PlayNotification(ModRules, null, "Sounds", "ClickSound", null);
				}
				else
				{
					YieldMouseFocus(mi);
					Game.Sound.PlayNotification(ModRules, null, "Sounds", "ClickDisabledSound", null);
				}
			}
			else if (mi.Event == MouseInputEvent.Move && HasMouseFocus)
				Depressed = RenderBounds.Contains(mi.Location);

			return Depressed;
		}

		public override void MouseEntered()
		{
			if (Info.TooltipContainer == null || GetTooltipText() == null)
				return;

			tooltipContainer.Value.SetTooltip(Info.TooltipTemplate,
				new WidgetArgs { { "button", this }, { "getText", GetTooltipText }, { "getDesc", GetTooltipDesc } });
		}

		public override void MouseExited()
		{
			if (Info.TooltipContainer == null || !tooltipContainer.IsValueCreated)
				return;

			tooltipContainer.Value.RemoveTooltip();
		}

		public override int2 ChildOrigin
		{
			get
			{
				return RenderOrigin +
					(Depressed ? new int2(Info.VisualHeight, Info.VisualHeight) : new int2(0, 0));
			}
		}

		public override void Draw()
		{
			var rb = RenderBounds;
			var disabled = IsDisabled();
			var highlighted = IsHighlighted();
			var font = Game.Renderer.Fonts[Info.Font];
			var text = GetText();
			var color = GetColor();
			var colordisabled = GetColorDisabled();
			var bgDark = GetContrastColorDark();
			var bgLight = GetContrastColorLight();
			var s = font.Measure(text);
			var stateOffset = Depressed ? new int2(Info.VisualHeight, Info.VisualHeight) : new int2(0, 0);
			var position = new int2(rb.X + (UsableWidth - s.X) / 2, rb.Y - Info.BaseLine + (Bounds.Height - s.Y) / 2);

			DrawBackground(rb, disabled, Depressed, Ui.MouseOverWidget == this, highlighted);
			if (Info.Contrast)
				font.DrawTextWithContrast(text, position + stateOffset,
					disabled ? colordisabled : color, bgDark, bgLight, 2);
			else if (Info.Shadow)
				font.DrawTextWithShadow(text, position, color, bgDark, bgLight, 1);
			else
				font.DrawText(text, position + stateOffset,
					disabled ? colordisabled : color);
		}

		public override Widget Clone() { return new ButtonWidget(this); }
		public virtual int UsableWidth { get { return Bounds.Width; } }

		public virtual void DrawBackground(Rectangle rect, bool disabled, bool pressed, bool hover, bool highlighted)
		{
			DrawBackground(Background, rect, disabled, pressed, hover, highlighted);
		}

		public static void DrawBackground(string baseName, Rectangle rect, bool disabled, bool pressed, bool hover, bool highlighted)
		{
			var variant = highlighted ? "-highlighted" : "";
			var state = disabled ? "-disabled" :
						pressed ? "-pressed" :
						hover ? "-hover" :
						"";

			WidgetUtils.DrawPanel(baseName + variant + state, rect);
		}
	}
}

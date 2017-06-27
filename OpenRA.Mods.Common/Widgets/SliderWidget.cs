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
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class SliderWidgetInfo : WidgetInfo
	{
		public readonly string Thumb = "slider-thumb";
		public readonly string Track = "slider-track";
		public readonly int Ticks = 0;
		public readonly int TrackHeight = 5;
		public readonly float MinimumValue = 0;
		public readonly float MaximumValue = 1;
		public readonly float Value = 0;

		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new SliderWidget(this, args, parent);
		}
	}

	public class SliderWidget : Widget
	{
		public new SliderWidgetInfo Info { get { return (SliderWidgetInfo)WidgetInfo; } }
		public Func<bool> IsDisabled = () => false;
		public event Action<float> OnChange = _ => { };
		public int Ticks = 0;
		public float Value = 0;
		public float MaximumValue = 1;
		public Func<float> GetValue;

		protected bool isMoving = false;

		public SliderWidget(SliderWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent)
		{
			Value = info.Value;
			MaximumValue = info.MaximumValue;
			Ticks = info.Ticks;
			GetValue = () => Value;
		}

		public SliderWidget(SliderWidget other)
			: base(other)
		{
			OnChange = other.OnChange;
			Value = other.Value;
			MaximumValue = other.MaximumValue;
			Ticks = other.Ticks;
			GetValue = other.GetValue;
		}

		void UpdateValue(float newValue)
		{
			Value = newValue.Clamp(Info.MinimumValue, MaximumValue);
			OnChange(Value);
		}

		public override bool HandleMouseInput(MouseInput mi)
		{
			if (mi.Button != MouseButton.Left) return false;
			if (IsDisabled()) return false;
			if (mi.Event == MouseInputEvent.Down && !TakeMouseFocus(mi)) return false;
			if (!HasMouseFocus) return false;

			switch (mi.Event)
			{
				case MouseInputEvent.Up:
					isMoving = false;
					YieldMouseFocus(mi);
					break;

				case MouseInputEvent.Down:
					isMoving = true;
					/* TODO: handle snapping to ticks properly again */
					/* TODO: handle nudge via clicking outside the thumb */
					UpdateValue(ValueFromPx(mi.Location.X - RenderBounds.Left));
					break;

				case MouseInputEvent.Move:
					if (isMoving)
						UpdateValue(ValueFromPx(mi.Location.X - RenderBounds.Left));
					break;
			}

			return ThumbRect.Contains(mi.Location);
		}

		float ValueFromPx(int x)
		{
			return Info.MinimumValue + (MaximumValue - Info.MinimumValue) * (x - 0.5f * RenderBounds.Height) / (RenderBounds.Width - RenderBounds.Height);
		}

		protected int PxFromValue(float x)
		{
			return (int)(0.5f * RenderBounds.Height + (RenderBounds.Width - RenderBounds.Height) * (x - Info.MinimumValue) / (MaximumValue - Info.MinimumValue));
		}

		public override Widget Clone() { return new SliderWidget(this); }

		Rectangle ThumbRect
		{
			get
			{
				var thumbPos = PxFromValue(Value);
				var rb = RenderBounds;
				var width = rb.Height;
				var height = rb.Height;
				var origin = (int)(rb.X + thumbPos - width / 2f);
				return new Rectangle(origin, rb.Y, width, height);
			}
		}

		public override void Draw()
		{
			if (!IsVisible())
				return;

			Value = GetValue();

			var tr = ThumbRect;
			var rb = RenderBounds;
			var trackWidth = rb.Width - rb.Height;
			var trackOrigin = rb.X + rb.Height / 2;
			var trackRect = new Rectangle(trackOrigin - 1, rb.Y + (rb.Height - Info.TrackHeight) / 2, trackWidth + 2, Info.TrackHeight);

			// Tickmarks
			var tick = ChromeProvider.GetImage("slider", "tick");
			for (var i = 0; i < Ticks; i++)
			{
				var tickPos = new float2(
					trackOrigin + (i * (trackRect.Width - (int)tick.Size.X) / (Ticks - 1)) - tick.Size.X / 2,
					trackRect.Bottom);

				WidgetUtils.DrawRGBA(tick, tickPos);
			}

			// Track
			WidgetUtils.DrawPanel(Info.Track, trackRect);

			// Thumb
			var thumbHover = Ui.MouseOverWidget == this && tr.Contains(Viewport.LastMousePos);
			ButtonWidget.DrawBackground(Info.Thumb, tr, IsDisabled(), isMoving, thumbHover, false);
		}
	}
}
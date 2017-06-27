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
using OpenRA.Graphics;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets
{
	public class RGBASpriteWidgetInfo : WidgetInfo
	{
		protected override Widget Construct(WidgetArgs args, Widget parent = null)
		{
			return new RGBASpriteWidget(this, args, parent);
		}
	}

	public class RGBASpriteWidget : Widget
	{
		public Func<Sprite> GetSprite = () => null;

		public RGBASpriteWidget(RGBASpriteWidgetInfo info, WidgetArgs args, Widget parent)
			: base(info, args, parent) { }

		protected RGBASpriteWidget(RGBASpriteWidget other)
			: base(other)
		{
			GetSprite = other.GetSprite;
		}

		public override Widget Clone() { return new RGBASpriteWidget(this); }

		public override void Draw()
		{
			var sprite = GetSprite();
			if (sprite != null)
				Game.Renderer.RgbaSpriteRenderer.DrawSprite(sprite, RenderOrigin);
		}
	}
}

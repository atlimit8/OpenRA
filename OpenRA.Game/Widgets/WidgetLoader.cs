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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenRA.Primitives;
using OpenRA.Widgets;

namespace OpenRA
{
	public class WidgetLoader
	{
		readonly Dictionary<string, MiniYamlNode> widgets = new Dictionary<string, MiniYamlNode>();
		readonly ConcurrentCache<string, WidgetInfo> widgetInfos;
		readonly HashSet<string> ids = new HashSet<string>();
		readonly ModData modData;

		public WidgetLoader(ModData modData)
		{
			this.modData = modData;
			widgetInfos = new ConcurrentCache<string, WidgetInfo>(LoadInfo);
			foreach (var file in modData.Manifest.ChromeLayout.Select(a => MiniYaml.FromStream(modData.DefaultFileSystem.Open(a), a)))
				foreach (var w in file)
				{
					var key = w.Key.Substring(w.Key.IndexOf('@') + 1);
					if (widgets.ContainsKey(key))
						throw new InvalidDataException("Widget has duplicate Key `{0}` at {1}".F(w.Key, w.Location));
					widgets.Add(key, w);
					ids.Add(key);
				}
		}

		WidgetInfo LoadInfo(string id)
		{
			return WidgetInfo.Load(widgets[id]);
		}

		// Needs to be initialized after ChromeMetrics so that the *WidgetInfo constructors can use the data
		public void Initialize()
		{
			Task.Run(() =>
			{
				foreach (var id in widgets.Keys)
					if (widgetInfos[id] == null)
						throw new InvalidDataException();

				widgets.Clear();
			});
		}

		public Widget CreateWidget(WidgetArgs args, Widget parent, string w)
		{
			if (!ids.Contains(w))
				throw new InvalidDataException("Cannot find widget with Id `{0}`".F(w));

			if (!args.ContainsKey("modData"))
				args = new WidgetArgs(args) { { "modData", modData } };

			return widgetInfos[w].Create(args, parent);
		}
	}
}
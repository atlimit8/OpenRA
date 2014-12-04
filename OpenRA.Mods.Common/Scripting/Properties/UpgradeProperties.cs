#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using OpenRA.Scripting;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Scripting
{
	[ScriptPropertyGroup("General")]
	public class UpgradeProperties : ScriptActorProperties, Requires<ConditionManagerInfo>
	{
		ConditionManager um;
		public UpgradeProperties(ScriptContext context, Actor self)
			: base(context, self)
		{
			um = self.Trait<ConditionManager>();
		}

		[Desc("Grant a condition level to this actor.")]
		public void GrantUpgrade(string upgrade)
		{
			um.GrantCondition(self, upgrade, this);
		}

		[Desc("Revoke a condition level that was previously granted using GrantUpgrade.")]
		public void RevokeUpgrade(string upgrade)
		{
			um.RevokeCondition(self, upgrade, this);
		}

		[Desc("Grant a limited-time condition level to this actor.")]
		public void GrantTimedUpgrade(string upgrade, int duration)
		{
			um.GrantTimedCondition(self, upgrade, duration);
		}

		[Desc("Check whether this actor accepts a specific condition.")]
		public bool AcceptsUpgrade(string upgrade)
		{
			return um.AcceptsConditionType(self, upgrade);
		}
	}
}
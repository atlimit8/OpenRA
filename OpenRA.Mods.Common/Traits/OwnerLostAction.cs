#region Copyright & License Information
/*
 * Copyright 2007-2021 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Primitives;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public enum OwnerLostActionType { ChangeOwner, Dispose, Kill }

	[Desc("Perform an action when the actor's owner is defeated.")]
	public class OwnerLostActionInfo : ConditionalTraitInfo
	{
		[FieldLoader.Require]
		[Desc("What does this unit do when its owner loses.",
		"Allowed values are {0")]
		[DescArg(DescArgType.EnumNamesWithOr)]
		public readonly OwnerLostActionType Action = OwnerLostActionType.Kill;

		[Desc("Map player to use when {0} is {1}.")]
		[DescArg(nameof(Action))]
		[DescArg(nameof(OwnerLostActionType.ChangeOwner))]
		public readonly string Owner = "Neutral";

		[Desc("The deathtypes used when {0} is {1}.")]
		[DescArg(nameof(Action))]
		[DescArg(nameof(OwnerLostActionType.Kill))]
		public readonly BitSet<DamageType> DeathTypes = default(BitSet<DamageType>);

		public override object Create(ActorInitializer init) { return new OwnerLostAction(this); }
	}

	public class OwnerLostAction : ConditionalTrait<OwnerLostActionInfo>, INotifyOwnerLost
	{
		public OwnerLostAction(OwnerLostActionInfo info)
			: base(info) { }

		void INotifyOwnerLost.OnOwnerLost(Actor self)
		{
			if (IsTraitDisabled)
				return;

			if (Info.Action == OwnerLostActionType.Kill)
				self.Kill(self, Info.DeathTypes);
			else if (Info.Action == OwnerLostActionType.Dispose)
				self.Dispose();
			else if (Info.Action == OwnerLostActionType.ChangeOwner)
				self.ChangeOwner(self.World.Players.First(p => p.InternalName == Info.Owner));
		}
	}
}

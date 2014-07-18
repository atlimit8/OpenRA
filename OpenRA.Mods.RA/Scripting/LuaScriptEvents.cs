#region Copyright & License Information
/*
 * Copyright 2007-2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Traits;

namespace OpenRA.Mods.RA.Scripting
{
	public class LuaScriptEventsInfo : TraitInfo<LuaScriptEvents> { }

	public class LuaScriptEvents : INotifyKilled, INotifyAddedToWorld, INotifyRemovedFromWorld,
		INotifyCapture, INotifyHostCapture, INotifyOwnerChanged, INotifyDamage, INotifyIdle, INotifyProduction
	{
		public event Action<Actor, AttackInfo> OnKilled = (self, e) => { };
		public event Action<Actor> OnAddedToWorld = self => { };
		public event Action<Actor> OnRemovedFromWorld = self => { };
		public event Action<Actor, Actor, Player, Player> BeforeCaptured = (self, captor, oldOwner, newOwner) => { };
		public event Action<Actor, Actor, Player, Player> AfterCaptured = (self, captor, oldOwner, newOwner) => { };
		public event Action<Actor, Player, Player> OwnerChanged = (self, oldOwner, newOwner) => { };
		public event Action<Actor, Actor, Actor> BeforeHostCaptured = (self, host, captor) => { };
		public event Action<Actor, Actor, Actor> AfterHostCaptured = (self, host, captor) => { };
		public event Action<Actor, AttackInfo> OnDamaged = (self, e) => { };
		public event Action<Actor> OnIdle = self => { };
		public event Action<Actor, Actor, CPos> OnProduced = (self, other, exit) => { };

		public void Killed(Actor self, AttackInfo e)
		{
			OnKilled(self, e);
		}

		public void AddedToWorld(Actor self)
		{
			OnAddedToWorld(self);
		}

		public void RemovedFromWorld(Actor self)
		{
			OnRemovedFromWorld(self);
		}

		public void BeforeCapture(Actor self, Actor captor, Player oldOwner, Player newOwner)
		{
			BeforeCaptured(self, captor, oldOwner, newOwner);
		}

		public void AfterCapture(Actor self, Actor captor, Player oldOwner, Player newOwner)
		{
			AfterCaptured(self, captor, oldOwner, newOwner);
		}

		public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
		{
			OwnerChanged(self, oldOwner, newOwner);
		}

		public void BeforeHostCapture(Actor self, Actor host, Actor captor)
		{
			BeforeHostCaptured(self, host, captor);
		}

		public void AfterHostCapture(Actor self, Actor host, Actor captor)
		{
			AfterHostCaptured(self, host, captor);
		}

		public void Damaged(Actor self, AttackInfo e)
		{
			OnDamaged(self, e);
		}

		public void TickIdle(Actor self)
		{
			OnIdle(self);
		}

		public void UnitProduced(Actor self, Actor other, CPos exit)
		{
			OnProduced(self, other, exit);
		}
	}
}

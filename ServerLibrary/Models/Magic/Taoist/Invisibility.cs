﻿using Library;
using Server.DBModels;
using Server.Envir;
using System;
using System.Drawing;
using System.Linq;

namespace Server.Models.Magic
{
    [MagicType(MagicType.Invisibility)]
    public class Invisibility : MagicObject
    {
        protected override Element Element => Element.None;
        public override bool UpdateCombatTime => false;

        public Invisibility(PlayerObject player, UserMagic magic) : base(player, magic)
        {

        }

        public override MagicCast MagicCast(MapObject target, Point location, MirDirection direction)
        {
            var response = new MagicCast
            {
                Ob = null
            };

            if (!Player.UseAmulet(2, 0))
            {
                response.Cast = false;
                return response;
            }

            var delay = SEnvir.Now.AddMilliseconds(500);

            ActionList.Add(new DelayedAction(delay, ActionType.DelayMagicNew, Type));

            return response;
        }

        public override void MagicComplete(params object[] data)
        {
            MapObject ob = (MapObject)data[1];

            if (ob?.Node == null || !Player.CanHelpTarget(ob) || ob.Buffs.Any(x => x.Type == BuffType.Invisibility)) return;

            Stats buffStats = new Stats
            {
                [Stat.Invisibility] = 1
            };

            ob.BuffAdd(BuffType.Invisibility, TimeSpan.FromSeconds((Magic.GetPower() + Player.GetSC() + Player.Stats[Stat.PhantomAttack] * 2)), buffStats, true, false, TimeSpan.Zero);

            Player.LevelMagic(Magic);
        }
    }
}
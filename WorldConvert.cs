﻿using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace WorldConvert
{
    internal enum Biome
    {
        normal,
        corruption,
        hallow,
        meteor,
        jungle,
    }
    [ApiVersion(1, 21)]
    public class WorldConvert : TerrariaPlugin
    {
        private Dictionary<ConvertPair, Dictionary<int, int>> conversions;

        public override string Author
        {
            get { return "Tshock Team"; }
        }

        public override string Description
        {
            get { return "Commands to convert different biomes."; }
        }

        public override string Name
        {
            get { return "World Convert"; }
        }

        public override Version Version
        {
            get { return new Version(1, 1); }
        }

        public WorldConvert(Main game) : base(game)
        {
            conversions = new Dictionary<ConvertPair, Dictionary<int, int>>(new EqualityComparer());

            ConvertPair p = new ConvertPair(Biome.hallow, Biome.normal);
            p.Add(117, 1);
            p.Add(109, 2);
            p.Add(116, 53);
            p.Add(110, 0);
            p.Add(118, 38);
            p.Add(115, 52);
            p.Add(113, 73);
            conversions.Add(p, p.Tiles);

            ConvertPair p1 = new ConvertPair(Biome.hallow, Biome.corruption);
            p1.Add(117, 25);
            p1.Add(109, 23);
            p1.Add(116, 112);
            p1.Add(110, 24);
            p1.Add(118, 38);
            p1.Add(115, 52);
            p1.Add(113, 0);
            conversions.Add(p1, p1.Tiles);

            ConvertPair p2 = new ConvertPair(Biome.corruption, Biome.normal);
            p2.Add(25, 1);
            p2.Add(23, 2);
            p2.Add(32, 0);
            p2.Add(24, 3);
            p2.Add(112, 53);
            conversions.Add(p2, p2.Tiles);

            ConvertPair p3 = new ConvertPair(Biome.corruption, Biome.hallow);
            p3.Add(25, 117);
            p3.Add(23, 109);
            p3.Add(32, 0);
            p3.Add(24, 110);
            p3.Add(112, 116);
            conversions.Add(p3, p3.Tiles);
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("wconvert", ConvertBiome, "convw"));
            Commands.ChatCommands.Add(new Command("wconvert", RemoveBiome, "remow"));
        }

        private void ConvertBiome(CommandArgs args)
        {
            if (args.Parameters.Count < 2)
            {
                args.Player.SendMessage("You must specify the biome you wish to convert and what to convert to.",
                                        Color.Red);
                return;
            }

            Biome inbiome, outbiome;

            if (!Biome.TryParse(args.Parameters[0].ToLower(), out inbiome))
            {
                args.Player.SendMessage(String.Format("{0} is not a valid biome", args.Parameters[0]), Color.Red);
                return;
            }

            if (!Biome.TryParse(args.Parameters[1].ToLower(), out outbiome))
            {
                args.Player.SendMessage(String.Format("{0} is not a valid biome", args.Parameters[1]), Color.Red);
                return;
            }

            if (!Convert(new ConvertPair(inbiome, outbiome)))
            {
                args.Player.SendMessage(String.Format("No conversion exists for {0} to {1}", args.Parameters[0], args.Parameters[1]), Color.Red);
            }
        }

        private bool Convert(ConvertPair pair)
        {
            if (!conversions.ContainsKey(pair))
            {
                return false;
            }

            Dictionary<int, int> tiles = conversions[pair];
            TShock.Utils.Broadcast("Server might lag for a moment.", Color.Red);
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    int type = Main.tile[x, y].type;

                    if (tiles.ContainsKey(Main.tile[x, y].type))
                    {
                        Main.tile[x, y].type = (byte) tiles[type];
                        if (tiles[type] == 0)
                        {
                            Main.tile[x, y].active(false);
                        }
                    }
                }
            }

            WorldGen.CountTiles(0);
            TSPlayer.All.SendData(PacketTypes.UpdateGoodEvil);
            Netplay.ResetSections();
            TShock.Utils.Broadcast("Conversion is complete.", Color.Green);
            return true;
        }

        private void RemoveBiome(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendMessage("You must specify the biome you wish to remove.", Color.Red);
                return;
            }

            Biome inbiome;

            if (!Biome.TryParse(args.Parameters[0], out inbiome))
            {
                args.Player.SendMessage(String.Format("{0} is not a valid biome", args.Parameters[0]), Color.Red);
                return;
            }

            if (!Convert(new ConvertPair(inbiome, Biome.normal)))
            {
                args.Player.SendMessage("Cannot remove that biome.", Color.Red);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }
    }

    internal class ConvertPair
    {
        private Biome inb;
        private Biome outb;
        private Dictionary<int, int> tiles;

        public ConvertPair(Biome inb, Biome outb)
        {
            From = inb;
            To = outb;
            tiles = new Dictionary<int, int>();
        }

        public Biome From
        {
            get { return inb; }
            set { inb = value; }
        }

        public Biome To
        {
            get { return outb; }
            set { outb = value; }
        }

        public Dictionary<int, int> Tiles
        {
            get 
            { return tiles; }
        }

        public override bool Equals(object obj)
        {
            return (obj.GetType() == typeof (ConvertPair)) && Equals((ConvertPair) obj);
        }

        public bool Equals(ConvertPair p)
        {
            return ((From == p.From) && (To == p.To));
        }

        public void Add(int i, int o)
        {
            tiles.Add(i, o);
        }
    }

    internal class EqualityComparer : IEqualityComparer<ConvertPair>
    {

        public bool Equals(ConvertPair x, ConvertPair y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ConvertPair x)
        {
            return (int)(x.To) * 13 + (int)(x.From) * 13;
        }
    }
}

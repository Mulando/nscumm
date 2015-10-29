﻿/*
 * This file is part of NScumm.
 * 
 * NScumm is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * NScumm is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with NScumm.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NScumm.Core.Audio;
using NScumm.Core.Graphics;
using NScumm.Core.Input;

namespace NScumm.Core.IO
{
    [Flags]
    public enum GameFeatures
    {
        None,
        SixteenColors = 0x01,
        Old256 = 0x02,
        FewLocals = 0x04,
        Demo = 0x08,
        Is16BitColor = 0x10,
        AudioTracks = 0x20,
    }

    public enum GameId
    {
        Maniac,
        Zak,
        Indy3,
        Indy4,
        Monkey1,
        Monkey2,
        Loom,
        Pass,
        Tentacle,
        SamNMax,
        FullThrottle,
        Dig,
        CurseOfMonkeyIsland
    }

    public enum Platform
    {
        DOS,
        Amiga,
        AtariST,
        Macintosh,
        FMTowns,
        Windows,
        NES,
        C64,
        CoCo3,
        Linux,
        Acorn,
        SegaCD,
        _3DO,
        PCEngine,
        Apple2GS,
        PC98,
        Wii,
        PSX,
        CDi,
        IOS,
        OS2,
        BeOS,

        Unknown = -1
    }

    public class ScummMetaEngine : IMetaEngine
    {
        private GameManager _gm;

        public ScummMetaEngine()
        {
            var resStream = typeof(GameManager).Assembly.GetManifestResourceStream(typeof(GameManager), "Nscumm.xml");
            _gm = GameManager.Create(resStream);
        }

        public IEngine Create(GameSettings settings, IGraphicsManager gfxManager, IInputManager inputManager, IAudioOutput output, bool debugMode = false)
        {
            return ScummEngine.Create(settings, gfxManager, inputManager, output, debugMode);
        }

        public GameDetected DetectGame(string path)
        {
            return new GameDetected(_gm.GetInfo(path), this);
        }
    }

    public class GameSettings
    {
        public IGameDescriptor Game { get; private set; }

        public IMetaEngine MetaEngine { get; private set; }

        public string AudioDevice { get; set; }

        public bool CopyProtection { get; set; }

        public int BootParam { get; set; }

        public GameSettings(IGameDescriptor game, IMetaEngine engine)
        {
            Game = game;
            AudioDevice = "adlib";
            MetaEngine = engine;
        }
    }

    public class GameInfo : IGameDescriptor
    {
        public Platform Platform { get; set; }

        public string Path { get; set; }

        public string Id { get; set; }

        public string Pattern { get; set; }

        public GameId GameId { get; set; }

        public string Variant { get; set; }

        public string Description { get; set; }

        public string MD5 { get; set; }

        public int Version { get; set; }

        public CultureInfo Culture { get; set; }

        public GameFeatures Features { get; set; }

        public MusicDriverTypes Music { get; set; }

        public bool IsOldBundle { get { return Version <= 3 && Features.HasFlag(GameFeatures.SixteenColors); } }

        public int Width
        {
            get
            {
                return Version == 8 ? 640 : 320;
            }
        }

        public int Height
        {
            get
            {
                if (Platform == Platform.FMTowns && Version == 3)
                {
                    return 240;
                }
                return Version == 8 ? 480 : 200;
            }
        }

        public PixelFormat PixelFormat
        {
            get
            {
                var format = Platform == Platform.FMTowns ? PixelFormat.Rgb16 : PixelFormat.Indexed8;
                return format;
            }
        }
    }

    public class GameManager
    {
        XDocument doc;
        static readonly XNamespace Namespace = "http://schemas.scemino.com/nscumm/2012/";

        public static GameManager Create(Stream stream)
        {
            var gm = new GameManager();
            gm.doc = XDocument.Load(stream);
            return gm;
        }

        public GameInfo GetInfo(string path)
        {
            GameInfo info = null;
            var signature = ServiceLocator.FileStorage.GetSignature(path);
            var gameMd5 = (from md5 in doc.Element(Namespace + "NScumm").Elements(Namespace + "MD5")
                           where (string)md5.Attribute("signature") == signature
                           select md5).FirstOrDefault();
            if (gameMd5 != null)
            {
                var game = (from g in doc.Element(Namespace + "NScumm").Elements(Namespace + "Game")
                            where (string)g.Attribute("id") == (string)gameMd5.Attribute("gameId")
                            where (string)g.Attribute("variant") == (string)gameMd5.Attribute("variant")
                            select g).FirstOrDefault();
                var desc = (from d in doc.Element(Namespace + "NScumm").Elements(Namespace + "Description")
                            where (string)d.Attribute("gameId") == (string)gameMd5.Attribute("gameId")
                            select (string)d.Attribute("text")).FirstOrDefault();
                var attFeatures = gameMd5.Attribute("features");
                var platformText = (string)gameMd5.Attribute("platform");
                var platform = platformText != null ? (Platform?)Enum.Parse(typeof(Platform), platformText, true) : null;
                var features = ParseFeatures((string)attFeatures);
                var attMusic = game.Attribute("music");
                var music = ParseMusic((string)attMusic);
                info = new GameInfo
                {
                    MD5 = signature,
                    Platform = platform.HasValue ? platform.Value : Platform.DOS,
                    Path = path,
                    Id = (string)game.Attribute("id"),
                    Pattern = (string)game.Attribute("pattern"),
                    GameId = (GameId)Enum.Parse(typeof(GameId), (string)game.Attribute("gameId"), true),
                    Variant = (string)game.Attribute("variant"),
                    Description = desc,
                    Version = (int)game.Attribute("version"),
                    Culture = new CultureInfo((string)gameMd5.Attribute("language")),
                    Features = features,
                    Music = music
                };
            }
            return info;
        }

        GameFeatures ParseFeatures(string feature)
        {
            var feat = feature == null ? new string[0] : feature.Split(' ');
            var features = GameFeatures.None;
            foreach (var f in feat)
            {
                features |= (GameFeatures)Enum.Parse(typeof(GameFeatures), f, true);
            }
            return features;
        }

        MusicDriverTypes ParseMusic(string music)
        {
            var mus = music == null ? new string[0] : music.Split(' ');
            var musics = MusicDriverTypes.None;
            foreach (var m in mus)
            {
                musics |= (MusicDriverTypes)Enum.Parse(typeof(MusicDriverTypes), m, true);
            }
            return musics;
        }
    }
}

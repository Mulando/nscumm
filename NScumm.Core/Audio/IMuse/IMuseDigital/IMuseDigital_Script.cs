﻿//
//  IMuseDigital_Script.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2015 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;
using NScumm.Core.IO;

namespace NScumm.Core.Audio.IMuse
{
    partial class IMuseDigital
    {
        public void ParseScriptCmds(int cmd, int b, int c, int d, int e, int f, int g, int h)
        {
            int soundId = b;
            int sub_cmd = c;

            if (cmd == 0)
                return;

            switch (cmd)
            {
                case 10: // ImuseStopAllSounds
                    StopAllSounds();
                    break;
                case 12: // ImuseSetParam
                    switch (sub_cmd)
                    {
                        case 0x400: // select group volume
                            SelectVolumeGroup(soundId, d);
                            break;
                        case 0x500: // set priority
                            SetPriority(soundId, d);
                            break;
                        case 0x600: // set volume
                            SetVolume(soundId, d);
                            break;
                        case 0x700: // set pan
                            SetPan(soundId, d);
                            break;
                        default:
                            Console.Error.WriteLine("DoCommand SetParam DEFAULT command {0}", sub_cmd);
                            break;
                    }
                    break;
                case 14: // ImuseFadeParam
                    switch (sub_cmd)
                    {
                        case 0x600: // set volume fading
                            if ((d != 0) && (e == 0))
                                SetVolume(soundId, d);
                            else if ((d == 0) && (e == 0))
                                StopSound(soundId);
                            else
                                SetFade(soundId, d, e);
                            break;
                        default:
                            Console.Error.WriteLine("doCommand FadeParam DEFAULT sub command %d", sub_cmd);
                            break;
                    }
                    break;
                case 25: // ImuseStartStream
                    Debug.WriteLine("ImuseStartStream ({0}, {1}, {2})", soundId, c, d);
                    break;
                case 26: // ImuseSwitchStream
                    Debug.WriteLine("ImuseSwitchStream ({0}, {1}, {2}, {3}, {4})", soundId, c, d, e, f);
                    break;
                case 0x1000: // ImuseSetState
                    Debug.WriteLine("ImuseSetState ({0})", b);
                    if ((_vm.Game.GameId == GameId.Dig) && (_vm.Game.Features.HasFlag(GameFeatures.Demo)))
                    {
                        if (b == 1)
                        {
                            FadeOutMusic(200);
                            StartMusic(1, 127);
                        }
                        else
                        {
                            if (GetSoundStatus(2) == 0)
                            {
                                FadeOutMusic(200);
                                StartMusic(2, 127);
                            }
                        }
                    }
                    else if ((_vm.Game.GameId == GameId.CurseOfMonkeyIsland) && (_vm.Game.Features.HasFlag(GameFeatures.Demo)))
                    {
                        if (b == 2)
                        {
                            FadeOutMusic(108);
                            StartMusic("in1.imx", 1100, 0, 127);
                        }
                        else if (b == 4)
                        {
                            FadeOutMusic(108);
                            StartMusic("in2.imx", 1120, 0, 127);
                        }
                        else if (b == 8)
                        {
                            FadeOutMusic(108);
                            StartMusic("out1.imx", 1140, 0, 127);
                        }
                        else if (b == 9)
                        {
                            FadeOutMusic(108);
                            StartMusic("out2.imx", 1150, 0, 127);
                        }
                        else if (b == 16)
                        {
                            FadeOutMusic(108);
                            StartMusic("gun.imx", 1210, 0, 127);
                        }
                        else
                        {
                            FadeOutMusic(120);
                        }
                    }
                    else if (_vm.Game.GameId == GameId.Dig)
                    {
                        SetDigMusicState(b);
                    }
                    else if (_vm.Game.GameId == GameId.CurseOfMonkeyIsland)
                    {
                        // TODO: vs
                        throw new NotImplementedException();
//                        SetComiMusicState(b);
                    }
                    else if (_vm.Game.GameId == GameId.FullThrottle)
                    {
                        SetFtMusicState(b);
                    }
                    break;
                case 0x1001: // ImuseSetSequence
                    Debug.WriteLine("ImuseSetSequence ({0})", b);
                    if (_vm.Game.GameId == GameId.Dig)
                    {
                        SetDigMusicSequence(b);
                    }
                    else if (_vm.Game.GameId == GameId.CurseOfMonkeyIsland)
                    {
                        // TODO: vs
                        throw new NotImplementedException();
                    }
                    else if (_vm.Game.GameId == GameId.FullThrottle)
                    {
                        SetFtMusicSequence(b);
                    }
                    break;
                case 0x1002: // ImuseSetCuePoint
                    Debug.WriteLine("ImuseSetCuePoint ({0})", b);
                    if (_vm.Game.GameId == GameId.FullThrottle)
                    {
                        SetFtMusicCuePoint(b);
                    }
                    break;
                case 0x1003: // ImuseSetAttribute
                    Debug.WriteLine("ImuseSetAttribute ({0}, {1})", b, c);
                    Debug.Assert((_vm.Game.GameId == GameId.Dig) || (_vm.Game.GameId == GameId.FullThrottle));
                    if (_vm.Game.GameId == GameId.Dig)
                    {
                        _attributes[b] = c;
                    }
                    break;
                case 0x2000: // ImuseSetGroupSfxVolume
                    break;
                case 0x2001: // ImuseSetGroupVoiceVolume
                    break;
                case 0x2002: // ImuseSetGroupMusicVolume
                    break;
                default:
                    throw new InvalidOperationException(string.Format("doCommand DEFAULT command {0}", cmd));
            }
        }

        void FlushTrack(Track track)
        {
            track.toBeRemoved = true;

            if (track.souStreamUsed)
            {
                _mixer.StopHandle(track.mixChanHandle);
            }
            else if (track.stream != null)
            {
                Debug.WriteLine("FlushTrack() - soundId:{0}", track.soundId);
                // Finalize the appendable stream, then remove our reference to it.
                // Note that there might still be some data left in the buffers of the
                // appendable stream. We play it nice and wait till all of it
                // played. The audio mixer will take care of it afterwards (and dispose it).
                track.stream.Finish();
                track.stream = null;
                if (track.soundDesc != null)
                {
                    _sound.CloseSound(track.soundDesc);
                    track.soundDesc = null;
                }
            }

            if (!_mixer.IsSoundHandleActive(track.mixChanHandle))
            {
                track.Clear();
            }
        }

        public void FlushTracks()
        {
            lock (_mutex)
            {
                Debug.WriteLine("flushTracks()");
                for (int l = 0; l < MAX_DIGITAL_TRACKS + MAX_DIGITAL_FADETRACKS; l++)
                {
                    Track track = _track[l];
                    if (track.used && track.toBeRemoved && !_mixer.IsSoundHandleActive(track.mixChanHandle))
                    {
                        Debug.WriteLine("flushTracks() - soundId:{0}", track.soundId);
                        track.Clear();
                    }
                }
            }
        }

        void refreshScripts()
        {
            lock (_mutex)
            {
                Debug.WriteLine("refreshScripts()");

                if (_stopingSequence != 0)
                {
                    // prevent start new music, only fade out old one
                    if (_vm.SmushActive)
                    {
                        FadeOutMusic(60);
                        return;
                    }
                    // small delay, it seems help for fix bug #1757010
                    if (_stopingSequence++ > 120)
                    {
                        Debug.WriteLine("refreshScripts() Force restore music state");
                        ParseScriptCmds(0x1001, 0, 0, 0, 0, 0, 0, 0);
                        _stopingSequence = 0;
                    }
                }

                bool found = false;
                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    Track track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.volGroupId == IMUSE_VOLGRP_MUSIC))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found && _curMusicState != 0)
                {
                    Debug.WriteLine("refreshScripts() Restore music state");
                    ParseScriptCmds(0x1001, 0, 0, 0, 0, 0, 0, 0);
                }
            }
        }

        public void StartVoice(int soundId, IAudioStream input)
        {
            Debug.WriteLine("StartVoiceStream({0})", soundId);
            StartSound(soundId, "", 0, IMUSE_VOLGRP_VOICE, input, 0, 127, 127, null);
        }

        public void StartVoice(int soundId, string soundName)
        {
            Debug.WriteLine("startVoiceBundle({0}, {1})", soundName, soundId);
            StartSound(soundId, soundName, IMUSE_BUNDLE, IMUSE_VOLGRP_VOICE, null, 0, 127, 127, null);
        }

        void StartMusic(int soundId, int volume)
        {
            Debug.WriteLine("startMusicResource({0})", soundId);
            StartSound(soundId, "", IMUSE_RESOURCE, IMUSE_VOLGRP_MUSIC, null, 0, volume, 126, null);
        }

        void StartMusic(string soundName, int soundId, int hookId, int volume)
        {
            Debug.WriteLine("startMusicBundle({0}, soundId:{1}, hookId:{2})", soundName, soundId, hookId);
            StartSound(soundId, soundName, IMUSE_BUNDLE, IMUSE_VOLGRP_MUSIC, null, hookId, volume, 126, null);
        }

        void StartMusicWithOtherPos(string soundName, int soundId, int hookId, int volume, Track otherTrack)
        {
            Debug.WriteLine("startMusicWithOtherPos({0}, soundId:{1}, hookId:{2}, oldSoundId:{3})", soundName, soundId, hookId, otherTrack.soundId);
            StartSound(soundId, soundName, IMUSE_BUNDLE, IMUSE_VOLGRP_MUSIC, null, hookId, volume, 126, otherTrack);
        }

        public void StartSfx(int soundId, int priority)
        {
            Debug.WriteLine("startSfx({0})", soundId);
            StartSound(soundId, "", IMUSE_RESOURCE, IMUSE_VOLGRP_SFX, null, 0, 127, priority, null);
        }

        void GetLipSync(int soundId, int syncId, int msPos, out int width, out int height)
        {
            int sync_size;
            byte[] sync_ptr;

            width = 0;
            height = 0;

            msPos /= 16;
            if (msPos < 65536)
            {
                lock (_mutex)
                {
                    for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                    {
                        var track = _track[l];
                        if (track.used && !track.toBeRemoved && (track.soundId == soundId))
                        {
                            _sound.GetSyncSizeAndPtrById(track.soundDesc, syncId, out sync_size, out sync_ptr);
                            var sync_pos = 0;
                            if ((sync_size != 0) && (sync_ptr != null))
                            {
                                sync_size /= 4;
                                while ((sync_size--) != 0)
                                {
                                    if (sync_ptr.ToUInt16BigEndian(sync_pos) >= msPos)
                                        break;
                                    sync_pos += 4;
                                }
                                if (sync_size < 0)
                                    sync_pos -= 4;
                                else if (sync_ptr.ToUInt16BigEndian(sync_pos) > msPos)
                                    sync_pos -= 4;

                                width = sync_ptr[2];
                                height = sync_ptr[3];
                                return;
                            }
                        }
                    }
                }
            }
        }

        int GetPosInMs(int soundId)
        {
            lock (_mutex)
            {
                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.soundId == soundId))
                    {
                        int pos = (5 * (track.dataOffset + track.regionOffset)) / (track.feedSize / 200);
                        return pos;
                    }
                }

                return 0;
            }
        }

        public int GetSoundStatus(int soundId)
        {
            lock (_mutex)
            {
                Debug.WriteLine("getSoundStatus({0})", soundId);
                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    // Note: We do not check track.toBeRemoved here on purpose (I *think*, at least).
                    // After all, tracks which are about to stop still are running (if only for a brief time).
                    if ((track.soundId == soundId) && track.used)
                    {
                        if (_mixer.IsSoundHandleActive(track.mixChanHandle))
                        {
                            return 1;
                        }
                    }
                }

                return 0;
            }
        }

        public void StopSound(int soundId)
        {
            lock (_mutex)
            {
                Debug.WriteLine("stopSound({0})", soundId);
                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.soundId == soundId))
                    {
                        Debug.WriteLine("stopSound({0}) - stopping sound", soundId);
                        FlushTrack(track);
                    }
                }
            }
        }

        int GetCurMusicPosInMs()
        {
            lock (_mutex)
            {
                int soundId = -1;

                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.volGroupId == IMUSE_VOLGRP_MUSIC))
                    {
                        soundId = track.soundId;
                    }
                }

                int msPos = GetPosInMs(soundId);
                Debug.WriteLine("getCurMusicPosInMs({0}) = {1}", soundId, msPos);
                return msPos;
            }
        }

        int GetCurVoiceLipSyncWidth()
        {
            lock (_mutex)
            {
                int msPos = GetPosInMs(Sound.TalkSoundID) + 50;
                int width = 0, height = 0;

                Debug.WriteLine("getCurVoiceLipSyncWidth({0})", Sound.TalkSoundID);
                GetLipSync(Sound.TalkSoundID, 0, msPos, out width, out height);
                return width;
            }
        }

        int getCurVoiceLipSyncHeight()
        {
            lock (_mutex)
            {
                int msPos = GetPosInMs(Sound.TalkSoundID) + 50;
                int width = 0, height = 0;

                Debug.WriteLine("getCurVoiceLipSyncHeight({0})", Sound.TalkSoundID);
                GetLipSync(Sound.TalkSoundID, 0, msPos, out width, out height);
                return height;
            }
        }

        int GetCurMusicLipSyncWidth(int syncId)
        {
            lock (_mutex)
            {
                int soundId = -1;

                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.volGroupId == IMUSE_VOLGRP_MUSIC))
                    {
                        soundId = track.soundId;
                    }
                }

                int msPos = GetPosInMs(soundId) + 50;
                int width = 0, height = 0;

                Debug.WriteLine("getCurVoiceLipSyncWidth({0}, {1})", soundId, msPos);
                GetLipSync(soundId, syncId, msPos, out width, out height);
                return width;
            }
        }

        int GetCurMusicLipSyncHeight(int syncId)
        {
            lock (_mutex)
            {
                int soundId = -1;

                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.volGroupId == IMUSE_VOLGRP_MUSIC))
                    {
                        soundId = track.soundId;
                    }
                }

                int msPos = GetPosInMs(soundId) + 50;
                int width = 0, height = 0;

                Debug.WriteLine("getCurVoiceLipSyncHeight({0}, {1})", soundId, msPos);
                GetLipSync(soundId, syncId, msPos, out width, out height);
                return height;
            }
        }

        public void StopAllSounds()
        {
            lock (_mutex)
            {
                Debug.WriteLine("stopAllSounds");

                for (int l = 0; l < MAX_DIGITAL_TRACKS + MAX_DIGITAL_FADETRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used)
                    {
                        // Stop the sound output, *now*. No need to use toBeRemoved etc.
                        // as we are protected by a mutex, and this method is never called
                        // from callback either.
                        _mixer.StopHandle(track.mixChanHandle);
                        if (track.soundDesc != null)
                        {
                            Debug.WriteLine("stopAllSounds - stopping sound({0})", track.soundId);
                            _sound.CloseSound(track.soundDesc);
                        }

                        // Mark the track as unused
                        track.Clear();
                    }
                }
            }
        }

        public void Pause(bool p)
        {
            _pause = p;
        }

        public void RefreshScripts()
        {
            lock (_mutex)
            {
                Debug.WriteLine("refreshScripts()");

                if (_stopingSequence != 0)
                {
                    // prevent start new music, only fade out old one
                    if (_vm.SmushActive)
                    {
                        FadeOutMusic(60);
                        return;
                    }
                    // small delay, it seems help for fix bug #1757010
                    if (_stopingSequence++ > 120)
                    {
                        Debug.WriteLine("refreshScripts() Force restore music state");
                        ParseScriptCmds(0x1001, 0, 0, 0, 0, 0, 0, 0);
                        _stopingSequence = 0;
                    }
                }

                bool found = false;
                for (int l = 0; l < MAX_DIGITAL_TRACKS; l++)
                {
                    var track = _track[l];
                    if (track.used && !track.toBeRemoved && (track.volGroupId == IMUSE_VOLGRP_MUSIC))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found && _curMusicState != 0)
                {
                    Debug.WriteLine("refreshScripts() Restore music state");
                    ParseScriptCmds(0x1001, 0, 0, 0, 0, 0, 0, 0);
                }
            }
        }

    }
}


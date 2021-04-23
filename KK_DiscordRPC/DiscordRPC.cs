﻿/*
 * "borrowed" from https://github.com/eai04191/craftopia-rpc/blob/main/craftopia-rpc/DiscordRPC.cs
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace KK_DiscordRPC
{
    internal class DiscordRPC
    {
        //DLL Imports here. Make sure the discord-rpc.dll is in the game's mono folder or else the game will crash.
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ReadyCallback();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DisconnectedCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ErrorCallback(int errorCode, string message);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void JoinCallback(string secret);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SpectateCallback(string secret);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RequestCallback(JoinRequest request);

        public struct EventHandlers
        {
            public ReadyCallback readyCallback;
            public DisconnectedCallback disconnectedCallback;
            public ErrorCallback errorCallback;
            public JoinCallback joinCallback;
            public SpectateCallback spectateCallback;
            public RequestCallback requestCallback;
        }

        [Serializable]
        public struct RichPresence
        {
            public string state;
            public string details;
            public long startTimestamp;
            public long endTimestamp;
            public string largeImageKey;
            public string largeImageText;
            public string smallImageKey;
            public string smallImageText;
            public string partyId;
            public int partySize;
            public int partyMax;
            public string matchSecret;
            public string joinSecret;
            public string spectateSecret;
            public bool instance;
        }

        [Serializable]
        public struct JoinRequest
        {
            public string userId;
            public string username;
            public string avatar;
        }

        public enum Reply
        {
            No,
            Yes,
            Ignore
        }

        [DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Initialize")]
        public static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

        [DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Shutdown")]
        public static extern void Shutdown();

        [DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_RunCallbacks")]
        public static extern void RunCallbacks();

        [DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_UpdatePresence")]
        public static extern void UpdatePresence(ref RichPresence presence);

        [DllImport("discord-rpc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Discord_Respond")]
        public static extern void Respond(string userId, Reply reply);
    }
}

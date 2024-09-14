﻿// SPDX-FileCopyrightText: © 2023 YorVeX, https://github.com/YorVeX
// SPDX-License-Identifier: MIT

using ObsInterop;
using System;
using System.Runtime.InteropServices;
using System.Text;
namespace xObsAsyncImageSource;

// original image source probably isn't changed to be threaded, see https://github.com/obsproject/obs-studio/issues/5444

public enum ObsLogLevel : int
{
    Error = ObsBase.LOG_ERROR,
    Warning = ObsBase.LOG_WARNING,
    Info = ObsBase.LOG_INFO,
    Debug = ObsBase.LOG_DEBUG
}

public static class Module
{

    const bool DebugLog = false; // enable for debug logging in OBS log
    const string DefaultLocale = "en-US";
    public static string ModuleName = "xObsAsyncImageSource";
    static string _locale = DefaultLocale;
    static unsafe obs_module* _obsModule = null;
    public static unsafe obs_module* ObsModule { get => _obsModule; }
    static unsafe text_lookup* _textLookupModule = null;
    static object _logLock = new Object();

    #region Helper methods
    public static unsafe void Log(string text, ObsLogLevel logLevel)
    {
        lock (_logLock)
        {
            if (DebugLog && (logLevel == ObsLogLevel.Debug))
                logLevel = ObsLogLevel.Info;
            // need to escape %, otherwise they are treated as format items, but since we provide null as arguments list this crashes OBS
            fixed (byte* logMessagePtr = Encoding.UTF8.GetBytes("[" + ModuleName + "] " + text.Replace("%", "%%")))
                ObsBase.blog((int)logLevel, (sbyte*)logMessagePtr);
        }
    }

    public static unsafe byte[] ObsText(string identifier, params object[] args)
    {
        return Encoding.UTF8.GetBytes(string.Format(ObsTextString(identifier), args));
    }

    public static unsafe byte[] ObsText(string identifier)
    {
        return Encoding.UTF8.GetBytes(ObsTextString(identifier));
    }

    public static unsafe string ObsTextString(string identifier, params object[] args)
    {
        return string.Format(ObsTextString(identifier), args);
    }

    public static unsafe string ObsTextString(string identifier)
    {
        fixed (byte* lookupVal = Encoding.UTF8.GetBytes(identifier))
        {
            sbyte* lookupResult = null;
            ObsTextLookup.text_lookup_getstr(_textLookupModule, (sbyte*)lookupVal, &lookupResult);
            var resultString = Marshal.PtrToStringUTF8((IntPtr)lookupResult);
            if (string.IsNullOrEmpty(resultString))
                return "<MissingLocale:" + _locale + ":" + identifier + ">";
            else
                return resultString;
        }
    }

    public static unsafe string GetString(sbyte* obsString)
    {
        string managedString = Marshal.PtrToStringUTF8((IntPtr)obsString)!;
        ObsBmem.bfree(obsString);
        return managedString;
    }
    #endregion Helper methods

    #region OBS module API methods
    [UnmanagedCallersOnly(EntryPoint = "obs_module_set_pointer", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static unsafe void SetPointer(obs_module* obsModulePointer)
    {
        ModuleName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name!;
        _obsModule = obsModulePointer;
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_ver", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static uint GetVersion()
    {
        var major = (uint)Obs.Version.Major;
        var minor = (uint)Obs.Version.Minor;
        var patch = (uint)Obs.Version.Build;
        var version = (major << 24) | (minor << 16) | patch;
        return version;
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_load", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static unsafe bool ModuleLoad()
    {
        Log("Loading module...", ObsLogLevel.Debug);
        AsyncImageSource.Register();
        var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
        Version version = assemblyName.Version!;
        Log("Version " + version.Major + "." + version.Minor + " loaded.", ObsLogLevel.Info);
        return true;
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_unload", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static unsafe void ModuleUnload()
    {
        Log("Unloading module...", ObsLogLevel.Debug);
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_set_locale", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static unsafe void ModuleSetLocale(char* locale)
    {
        Log("ModuleSetLocale called", ObsLogLevel.Debug);
        var localeString = Marshal.PtrToStringUTF8((IntPtr)locale);
        if (!string.IsNullOrEmpty(localeString))
        {
            _locale = localeString;
            Log("Locale is set to: " + _locale, ObsLogLevel.Debug);
        }
        if (_textLookupModule != null)
            ObsTextLookup.text_lookup_destroy(_textLookupModule);
        fixed (byte* defaultLocale = Encoding.UTF8.GetBytes(DefaultLocale), currentLocale = Encoding.UTF8.GetBytes(_locale))
            _textLookupModule = Obs.obs_module_load_locale(_obsModule, (sbyte*)defaultLocale, (sbyte*)currentLocale);
    }

    [UnmanagedCallersOnly(EntryPoint = "obs_module_free_locale", CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvCdecl) })]
    public static unsafe void ModuleFreeLocale()
    {
        if (_textLookupModule != null)
            ObsTextLookup.text_lookup_destroy(_textLookupModule);
        _textLookupModule = null;
        Log("ModuleFreeLocale called", ObsLogLevel.Debug);
    }



    #endregion OBS module API methods

}
require "Engine/PlatformBindings/modules/digbuild"

workspace "DigBuild"
    configurations { "Debug", "Release" }
    startproject "DigBuild"

    include "Engine/PlatformBindings/PlatformCPP"
    include "Engine/PlatformBindings/PlatformCS"
    include "Engine/PlatformBindings/PlatformSourceGen"
    include "Engine/Engine"
    include "Game"

project "DigBuild.Platform"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"
project "DigBuild.Platform.Native"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"
project "DigBuild.Platform.SourceGen"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"
project "DigBuild.Engine"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"

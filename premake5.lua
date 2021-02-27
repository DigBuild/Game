require "Engine/PlatformBindings/modules/digbuild"

workspace "DigBuildGame"
    configurations { "Debug", "Release" }
    startproject "DigBuildGame"

    include "Engine/PlatformBindings/PlatformCPP"
    include "Engine/PlatformBindings/PlatformCS"
    include "Engine/PlatformBindings/PlatformSourceGen"
    include "Engine/Engine"
    include "Game"

project "DigBuildPlatformCPP"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"
project "DigBuildPlatformCS"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"
project "DigBuildEngine"
    targetdir "bin/%{cfg.buildcfg}"
    objdir "bin-int/%{cfg.buildcfg}"

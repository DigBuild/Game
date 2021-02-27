project "DigBuild"
    kind "ConsoleApp"
    framework "net5.0"
    language "C#"
    csversion "9.0"
    enabledefaultcompileitems(true)
    allownullable(true)
    noframeworktag(true)
    targetdir "../bin/%{cfg.buildcfg}"
    objdir "../bin-int/%{cfg.buildcfg}"
    -- resourcesdir "Resources"

    dependson {
		"DigBuildEngine",
		"DigBuildPlatformSourceGen"
	}
    links {
		"DigBuildEngine"
	}
	analyzer {
		"DigBuildPlatformSourceGen"
	}

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"

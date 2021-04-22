project "DigBuild.Content"
    kind "SharedLib"
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
		"DigBuild"
	}
    links {
		"DigBuild"
	}

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"

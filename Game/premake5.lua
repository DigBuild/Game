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
		"DigBuild.Engine",
		"DigBuild.Platform.SourceGen"
	}
    links {
		"DigBuild.Engine"
	}
	analyzer {
		"DigBuild.Platform.SourceGen"
	}
    nuget {
        "CjClutter.ObjLoader:1.0.0"
    }

    filter "configurations:Debug"
        defines { "DB_DEBUG" }
        symbols "On"

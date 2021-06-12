#version 450
#extension GL_ARB_separate_shader_objects : enable

const vec3 waterColorDark  = vec3(0.0, 0.04, 0.2);
const vec3 waterColorLight = vec3(0.05, 0.1, 0.5);

const vec3 skyColorNight = vec3(0.000, 0.000, 0.005);
const vec3 skyColorDay   = vec3(0.150, 0.600, 0.900);

const vec3 tintDark  = vec3(0.109, 0.121, 0.180);
const vec3 tintLight = vec3(1.000, 1.000, 1.000);

layout(binding = 0) uniform UBO {
    mat4 invProjection;
    vec4 fogColor;
    // bit 0 => underwater
    int flags;
    float timeFactor;
};

layout(set = 1, binding = 1) uniform sampler2D worldTex;
layout(set = 2, binding = 2) uniform sampler2D positionTex;
layout(set = 3, binding = 3) uniform sampler2D waterTex;

layout(location = 0) in vec2 fragUV;

layout(location = 0) out vec4 outColor;

void applyWater(inout vec3 worldColor, inout vec3 worldPos, inout vec4 waterData) {
    if (waterData.a == 0) {
        return;
    }
    
    vec3 tint = mix(tintDark, tintLight, timeFactor);

    if (length(worldPos) == 0) {
        worldColor = waterColorDark * tint;
        return;
    }
    
    float depthDiff = waterData.r - worldPos.z;
    depthDiff = clamp(exp(-depthDiff / 2), 0, 1);
    if (depthDiff == 1) {
        worldColor = waterColorDark * tint;
        worldPos = vec3(worldPos.xy, 0);
        return;
    }

    vec3 waterColor = mix(waterColorDark, waterColorLight, depthDiff) * tint;

    worldColor = mix(worldColor, waterColor, 0.9 - depthDiff * 0.3);
    worldPos = vec3(worldPos.xy, waterData.r);
}

void applyFog(inout vec3 worldColor, inout vec3 worldPos, inout vec4 waterData, inout vec4 fogCol) {
    float depth = mix(worldPos.z, waterData.r, waterData.a);
    float fogFactor = 1 - exp(-pow(-depth, 3.0) / 50000);
    if (length(worldPos) == 0 && waterData.a == 0)
        fogFactor = 0;

    vec3 skyColor = mix(skyColorNight, skyColorDay, timeFactor);
    vec3 actualFogColor = mix(skyColor, fogCol.rgb, fogCol.a);

    worldColor = mix(worldColor, actualFogColor, fogFactor);
}

void applyUnderwater(inout vec3 worldColor, inout vec4 fogCol) {
    // Return if no underwater flag
    if ((flags & (1 << 0)) == 0) return;
    
    vec3 tint = mix(tintDark, tintLight, timeFactor);

    worldColor = mix(worldColor, waterColorDark * tint, 0.7);
    fogCol = vec4(mix(waterColorDark, waterColorLight, timeFactor) * tint, 1.0);
}

void main() {
    vec3 worldColor = texture(worldTex, fragUV).rgb;
    vec3 worldPos = texture(positionTex, fragUV).rgb;
    vec4 waterData = texture(waterTex, fragUV);
    
    vec4 fogCol = fogColor;

    applyWater(worldColor, worldPos, waterData);
    applyUnderwater(worldColor, fogCol);
    applyFog(worldColor, worldPos, waterData, fogCol);

    outColor = vec4(worldColor, 1.0);
}
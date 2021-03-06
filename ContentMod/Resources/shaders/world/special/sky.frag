#version 450
#extension GL_ARB_separate_shader_objects : enable

#define PI 3.14159265359

const vec3 starColor1 = vec3(0.271, 0.522, 0.692);
const vec3 starColor2 = vec3(0.798, 0.765, 0.606);

const vec3 skyColorNight = vec3(0.000, 0.000, 0.005);
const vec3 skyColorDay   = vec3(0.150, 0.600, 0.900);

const float oneOverSqrt2 = 0.70710678118;
const vec3 moonPosition = vec3(0, oneOverSqrt2, oneOverSqrt2);

layout(set = 1, binding = 0) uniform UBO {
    float timeFactor;
};

layout(location = 0) in vec3 fragPosition;
layout(location = 1) in vec3 fragNormal;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 outBloomColor;
layout(location = 2) out vec4 outWater;
layout(location = 3) out vec4 outNormal;
layout(location = 4) out vec4 outPosition;

float hash21(vec2 p) {
    p = fract(p * vec2(3573.25, 92052.153));
    p += dot(p, p + 46.35);
    return fract(p.x * p.y);
}

vec3 grid(vec3 p, float subdivisions) {
    vec3 angle = vec3(
        atan(p.y / p.z),
        atan(p.x / p.z),
        atan(p.x / p.y)
    ) / PI * subdivisions * 2;

    vec3 pa = abs(p);
    if (pa.x > pa.y && pa.x > pa.z) {
        return vec3(angle.y, angle.z, sign(p.x) * 0.5 + 0.5);
    } else if (pa.y > pa.x && pa.y > pa.z) {
        return vec3(angle.x, angle.z, 2 + sign(p.y) * 0.5 + 0.5);
    } else {
        return vec3(angle.x, angle.y, 4 + sign(p.z) * 0.5 + 0.5);
    }
}

float star(vec2 p, float size) {
    return 1 - smoothstep(0, size, length(p));
}

vec3 starfield(vec3 sphereNormal, float subdivisions) {
    vec3 gridPos = grid(sphereNormal, subdivisions);
    vec2 gridPosUnique = gridPos.xy + gridPos.z * subdivisions;
    vec2 gridCoord = floor(gridPosUnique);
    vec2 gridOff = fract(gridPosUnique);
    float rand = hash21(gridCoord * 0.05);

    vec2 off = vec2(rand, fract(rand * 32.47)) * 0.8 - 0.4;
    float size = fract(rand * 325.727);
    float brightness = fract(rand * 9428.851) * step(0.4, size);
    float hue = fract(rand * 2035.143);
    
    return star(gridOff - 0.5 + off, size * 0.05) * brightness * mix(starColor1, starColor2, hue);
}

void main() {
    vec3 sphereNormal = normalize(fragNormal);

    vec3 skyColor = mix(skyColorNight, skyColorDay, timeFactor);

    vec3 color = skyColor;
    if (timeFactor < 0.5) {
        vec3 stars = vec3(0);
        stars += starfield(sphereNormal, 20);
        stars += starfield(sphereNormal.yzx, 12) * 0.5;
        stars += starfield(sphereNormal.zxy, 40) * 0.4;
        color += pow(stars, vec3(1.5)) * (0.5 - timeFactor) * 2;
    }

    float moonLit = smoothstep(0.998, 0.9983, dot(sphereNormal, moonPosition));
    float moonCover = smoothstep(0.9983, 0.9985, dot(normalize(sphereNormal + vec3(0.04, 0, -0.01)), moonPosition));

    color = mix(color, skyColor - 0.01, moonLit);
    color += max(0, moonLit - moonCover) * vec3(1, 0.7, 0.6);
    
    outColor = vec4(color, 1); //vec4(0.15, 0.6, 0.9, 1.0);
    outBloomColor = outWater = vec4(0);
    outNormal = vec4(sphereNormal, 1.0);
    outPosition = vec4(0, 0, 0, 1.0);
}

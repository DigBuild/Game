#version 450
#extension GL_ARB_separate_shader_objects : enable

const vec3 color1 = vec3(0.271, 0.522, 0.692);
const vec3 color2 = vec3(0.798, 0.765, 0.606);

const float oneOverSqrt2 = 0.70710678118;
const vec3 moonPosition = vec3(0, oneOverSqrt2, oneOverSqrt2);

layout(binding = 0) uniform UBO {
	mat4 matrix;
    float aspectRatio;
};

layout(location = 0) in vec2 fragPos;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 bloomColor;

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
    ) / 3.1415 * subdivisions * 2;

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
    
    return star(gridOff - 0.5 + off, size * 0.05) * brightness * mix(color1, color2, hue);
}

void main() {
    vec3 sphereNormal = normalize((matrix * vec4(fragPos, 1, 0)).xyz);

    vec3 baseColor = vec3(0, 0, 0.005);
    vec3 color = baseColor;
    color += starfield(sphereNormal, 20);

    float moonLit = smoothstep(0.998, 0.9983, dot(sphereNormal, moonPosition));
    float moonCover = smoothstep(0.9983, 0.9985, dot(normalize(sphereNormal + vec3(0.04, 0, -0.01)), moonPosition));

    color = mix(color, baseColor * 0.5, moonLit);
    color += max(0, moonLit - moonCover) * vec3(1, 0.7, 0.6);
    
    outColor = vec4(color, 1); //vec4(0.15, 0.6, 0.9, 1.0);
    bloomColor = vec4(0);
}

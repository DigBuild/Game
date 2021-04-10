#version 450
#extension GL_ARB_separate_shader_objects : enable

const vec3 tintDark  = vec3(0.109, 0.121, 0.180);
const vec3 tintLight = vec3(1.000, 1.000, 1.000);
const float sunlight = 0.0;

layout(set = 1, binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec2 fragBloomUV;
layout(location = 2) in vec3 fragNormal;
layout(location = 3) in float fragBrightness;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 bloomColor;

float calculateNormalShade(vec3 normal) {
    return min(0.7 - 0.2 * abs(normal.x) + 0.5 * normal.y, 1.0);
}

void main() {
    vec4 color = texture(tex, fragUV);
    if (color.a == 0)
        discard;
    vec4 bloom = texture(tex, fragBloomUV);
    float shade = max(calculateNormalShade(fragNormal) * (0.5 + fragBrightness * 0.5), bloom.a);
    outColor = vec4(color.rgb * shade * mix(tintDark, tintLight, max(max(sunlight, bloom.a), fragBrightness)), 1);
    bloomColor = vec4(bloom.rgb, 1);
}

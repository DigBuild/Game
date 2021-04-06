#version 450
#extension GL_ARB_separate_shader_objects : enable

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
    vec4 bloom = texture(tex, fragBloomUV);
    float shade = max(calculateNormalShade(fragNormal) * (0.25 + fragBrightness * 0.75), bloom.a);
    outColor = vec4(color.rgb * shade, color.a);
    bloomColor = vec4(bloom.rgb, 1);
}

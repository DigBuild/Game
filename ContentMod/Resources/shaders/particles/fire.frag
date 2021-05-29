#version 450
#extension GL_ARB_separate_shader_objects : enable

const float threshold = 0.1;
const float invThreshold = 1 - threshold;
const float veryOldThreshold = 0.98;

const vec3 tintVeryYoung  = vec3(1, 0, 0.005);
const vec3 tintYoung  = vec3(1, 0, 0);
const vec3 tintOld = vec3(1, 1, 0);

layout(set = 1, binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 fragUV;
layout(location = 1) in flat float fragAge;

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 bloomColor;

void main() {
    vec3 tint = tintVeryYoung;
    tint = mix(tint, tintYoung, min(fragAge / threshold, 1));
    tint = mix(tint, tintOld, (fragAge - threshold) / invThreshold);

    float fade = sin(fragAge * 20) * 0.4 + 0.6;
    float fadeOut = min((veryOldThreshold - fragAge) / veryOldThreshold, 1);

    vec4 color = texture(tex, fragUV) * vec4(tint, 1.0) * fade * fadeOut;

    outColor = bloomColor = color * 0.2;
}

#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(set = 1, binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 fragSampledUVs[5];

layout(location = 0) out vec4 outColor;

void main() {
    outColor =
        texture(tex, fragSampledUVs[0]) * 0.06136 +
        texture(tex, fragSampledUVs[1]) * 0.24477 +
        texture(tex, fragSampledUVs[2]) * 0.38774 +
        texture(tex, fragSampledUVs[3]) * 0.24477 +
        texture(tex, fragSampledUVs[4]) * 0.06136;
}
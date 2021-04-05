#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) out vec4 outColor;
layout(location = 1) out vec4 bloomColor;

void main() {
    outColor = vec4(0.15, 0.6, 0.9, 1.0);
    bloomColor = vec4(0);
}

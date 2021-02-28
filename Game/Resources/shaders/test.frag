#version 450
#extension GL_ARB_separate_shader_objects : enable

/* layout(set = 2, binding = 0) uniform sampler2D tex; */

layout(location = 0) in vec2 fragUV;
layout(location = 1) in vec3 fragNormal;

layout(location = 0) out vec4 outColor;

float calculateNormalShade(vec3 normal) {
    return min(0.7 - 0.2 * abs(normal.x) + 0.5 * normal.y, 1.0);
}

void main() {
    vec4 color = vec4(fragUV, 1.0, 1.0); /* texture(tex, fragUV); */
    float shade = calculateNormalShade(fragNormal);
    outColor = vec4(color.rgb * shade, color.a);
}

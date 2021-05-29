#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform sampler2D tex;

layout(location = 0) in vec2 fragUV;

layout(location = 0) out vec4 outColor;

void main() {
    const float gamma = 2.2;
    const float exposure = 1;

    vec3 hdrColor = texture(tex, fragUV).rgb;
    
    vec3 mapped = vec3(1) - exp(-hdrColor * exposure);
    //mapped = pow(mapped, vec3(1.0 / gamma));

    outColor = vec4(mapped, 1.0);
}